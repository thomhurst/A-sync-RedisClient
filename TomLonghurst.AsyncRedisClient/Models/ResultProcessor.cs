using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Helpers;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public abstract class ResultProcessor
    {

    }

    public abstract class ResultProcessor<T> : ResultProcessor
    {
        private Client.RedisClient _redisClient;
        protected ReadResult ReadResult;
        protected PipeReader PipeReader;

        public IRedisCommand LastCommand { get => _redisClient.LastCommand; set => _redisClient.LastCommand = value; }

        public string LastAction { get => _redisClient.LastAction; set => _redisClient.LastAction = value; }

        private void SetMembers(Client.RedisClient redisClient, PipeReader pipeReader)
        {
            _redisClient = redisClient;
            PipeReader = pipeReader;
        }

        internal async ValueTask<T> Start(Client.RedisClient redisClient, PipeReader pipeReader)
        {
            SetMembers(redisClient, pipeReader);

            ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);

            return await Process();
        }

        private protected abstract ValueTask<T> Process();


        protected async ValueTask<Memory<byte>> ReadData()
        {
            var buffer = ReadResult.Buffer;

            if (buffer.IsEmpty)
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var line = await ReadLine();

            var firstChar = line.ItemAt(0);

            if (firstChar != '$')
            {
                var stringLine = buffer.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);

                if (firstChar == '-')
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }

                throw new UnexpectedRedisResponseException($"Unexpected reply: {stringLine}");
            }

            var alreadyReadToLineTerminator = false;

            var byteSizeOfData = NumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

            if (byteSizeOfData == -1)
            {
                return null;
            }

            LastAction = "Reading Data Synchronously in ReadData";
            LastAction = "Reading Data Asynchronously in ReadData";
            ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);

            buffer = ReadResult.Buffer;

            if (byteSizeOfData == 0)
            {
                throw new UnexpectedRedisResponseException("Invalid length");
            }

            var bytes = new byte[byteSizeOfData].AsMemory();

            buffer = buffer.Slice(0, Math.Min(byteSizeOfData, buffer.Length));

            var bytesReceived = buffer.Length;

            buffer.CopyTo(bytes.Slice(0, (int) bytesReceived).Span);

            if (bytesReceived == byteSizeOfData)
            {
                alreadyReadToLineTerminator = TryAdvanceToLineTerminator(ref buffer);
            }
            else
            {
                PipeReader.AdvanceTo(buffer.End);
            }

            while (bytesReceived < byteSizeOfData)
            {
                LastAction = "Advancing Buffer in ReadData Loop";

                LastAction = "Reading Data Synchronously in ReadData Loop";
                LastAction = "Reading Data Asynchronously in ReadData Loop";
                ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);

                buffer = ReadResult.Buffer.Slice(0,
                    Math.Min(ReadResult.Buffer.Length, byteSizeOfData - bytesReceived));

                buffer
                    .CopyTo(bytes.Slice((int) bytesReceived,
                        (int) Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).Span);

                bytesReceived += buffer.Length;

                if (bytesReceived == byteSizeOfData)
                {
                    alreadyReadToLineTerminator = TryAdvanceToLineTerminator(ref buffer);
                }
                else
                {
                    PipeReader.AdvanceTo(buffer.End);
                }
            }

            if (!alreadyReadToLineTerminator)
            {
                LastAction = "Reading Data Asynchronously in ReadData Loop";
                ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);

                await PipeReader.AdvanceToLineTerminator(ReadResult);
            }

            return bytes;
        }

        private bool TryAdvanceToLineTerminator(ref ReadOnlySequence<byte> buffer)
        {
            var slicedBytes = ReadResult.Buffer.Slice(buffer.End);
            if (slicedBytes.IsEmpty)
            {
                PipeReader.AdvanceTo(buffer.End);
                return false;
            }

            var endOfLinePosition = slicedBytes.GetEndOfLinePosition();
            if (endOfLinePosition == null)
            {
                PipeReader.AdvanceTo(buffer.End);
                return false;
            }

            PipeReader.AdvanceTo(endOfLinePosition.Value);
            return true;
        }

        protected async Task<ReadOnlySequence<byte>> ReadLine()
        {
            LastAction = "Finding End of Line Position";
            var endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            if (endOfLinePosition == null)
            {
                LastAction = "Reading until End of Line found";

                ReadResult = await PipeReader.ReadUntilEndOfLineFound(ReadResult);

                LastAction = "Finding End of Line Position";
                endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            }

            if (endOfLinePosition == null)
            {
                throw new RedisDataException("Can't find EOL in ReadLine");
            }

            var buffer = ReadResult.Buffer;

            return buffer.Slice(0, endOfLinePosition.Value);
        }
    }

    public class SuccessResultProcessor : ResultProcessor<object>
    {
        private protected override async ValueTask<object> Process()
        {
            var buffer = await ReadLine();

            if (buffer.Length < 3 ||
                buffer.ItemAt(0) != '+' ||
                buffer.ItemAt(1) != 'O' ||
                buffer.ItemAt(2) != 'K')
            {
                var line = buffer.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(buffer.End);
                throw new RedisFailedCommandException(line, LastCommand);
            }

            PipeReader.AdvanceTo(buffer.End);

            return null;
        }
    }

    public class DataResultProcessor : ResultProcessor<string>
    {
        private protected override async ValueTask<string> Process()
        {
            return (await ReadData()).AsString();
        }
    }

    public class WordResultProcessor : ResultProcessor<string>
    {
        private protected override async ValueTask<string> Process()
        {
            var buffer = await ReadLine();

            if (buffer.ItemAt(0) != '+')
            {
                var stringLine = buffer.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(buffer.End);
                throw new UnexpectedRedisResponseException(stringLine);
            }

            var word = buffer.Slice(1, buffer.Length - 1).AsStringWithoutLineTerminators();
            PipeReader.AdvanceTo(buffer.End);
            return word;
        }
    }

    public class IntegerResultProcessor : ResultProcessor<int>
    {
        private protected override async ValueTask<int> Process()
        {
            var buffer = await ReadLine();

            if (buffer.ItemAt(0) != ':')
            {
                var invalidResponse = buffer.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(buffer.End);
                throw new UnexpectedRedisResponseException(invalidResponse);
            }

            var number = NumberParser.Parse(buffer);

            PipeReader.AdvanceTo(buffer.End);

            if (number == -1)
            {
                return -1;
            }

            return (int) number;
        }
    }

    public class FloatResultProcessor : ResultProcessor<float>
    {
        private protected override async ValueTask<float> Process()
        {
            var floatString = (await ReadData()).AsString();

            if (!float.TryParse(floatString, out var number))
            {
                throw new UnexpectedRedisResponseException(floatString);
            }

            return number;
        }
    }

    public class ArrayResultProcessor : ResultProcessor<IEnumerable<StringRedisValue>>
    {
        private protected override async ValueTask<IEnumerable<StringRedisValue>> Process()
        {
            var buffer = await ReadLine();

            if (buffer.ItemAt(0) != '*')
            {
                var stringLine = buffer.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(buffer.End);
                throw new UnexpectedRedisResponseException(stringLine);
            }

            var count = NumberParser.Parse(buffer);
            
            PipeReader.AdvanceTo(buffer.End);

            var results = new byte [count][];
            for (var i = 0; i < count; i++)
            {
                // Refresh the pipe buffer before 'ReadData' method reads it
                LastAction = "Reading Data Synchronously in ExpectArray";
                LastAction = "Reading Data Asynchronously in ExpectArray";
                ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);

                results[i] = (await ReadData()).ToArray();
            }

            return results.ToRedisValues();
        }
    }
}