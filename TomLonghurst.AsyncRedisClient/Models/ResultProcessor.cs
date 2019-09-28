using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Helpers;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public abstract class ResultProcessor
    {
        protected Client.RedisClient RedisClient;
        protected ReadResult ReadResult;
        protected PipeReader PipeReader;
        protected CancellationToken CancellationToken;

        internal void SetMembers(Client.RedisClient redisClient, PipeReader pipeReader,
            CancellationToken cancellationToken)
        {
            RedisClient = redisClient;
            PipeReader = pipeReader;
        }

        internal void SetMembers(Client.RedisClient redisClient, PipeReader pipeReader, ReadResult readResult,
            CancellationToken cancellationToken)
        {
            RedisClient = redisClient;
            PipeReader = pipeReader;
            ReadResult = readResult;
        }
    }

    public abstract class ResultProcessor<T> : ResultProcessor
    {
        public IRedisCommand LastCommand
        {
            get => RedisClient.LastCommand;
            set => RedisClient.LastCommand = value;
        }

        public string LastAction
        {
            get => RedisClient.LastAction;
            set => RedisClient.LastAction = value;
        }

        internal async ValueTask<T> Start(Client.RedisClient redisClient, PipeReader pipeReader,
            CancellationToken cancellationToken)
        {
            SetMembers(redisClient, pipeReader, cancellationToken);

            if (!PipeReader.TryRead(out ReadResult))
            {
                ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken).ConfigureAwait(false);
            }

            return await Process();
        }

        internal abstract ValueTask<T> Process();


        protected async ValueTask<Memory<byte>> ReadData()
        {
            var buffer = ReadResult.Buffer;

            if (buffer.IsEmpty)
            {
                throw new RedisDataException("Zero Length Response from Redis");
            }

            var line = await ReadLine();

            var firstChar = line.ItemAt(0);

            if (firstChar == '-')
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                throw new RedisFailedCommandException(stringLine, LastCommand);
            }

            if (firstChar != '$')
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                throw new UnexpectedRedisResponseException($"Unexpected reply: {stringLine}");
            }

            var alreadyReadToLineTerminator = false;

            var byteSizeOfData = NumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

            if (byteSizeOfData == -1)
            {
                return null;
            }

            if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
            {
                return default;
            }

            LastAction = "Reading Data Synchronously in ReadData";
            if (!PipeReader.TryRead(out ReadResult))
            {
                LastAction = "Reading Data Asynchronously in ReadData";
                ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);
            }

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
                if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
                {
                    return default;
                }

                if (ReadResult.IsCanceled)
                {
                    return default;
                }

                LastAction = "Advancing Buffer in ReadData Loop";

                LastAction = "Reading Data Synchronously in ReadData Loop";
                if (!PipeReader.TryRead(out ReadResult))
                {
                    LastAction = "Reading Data Asynchronously in ReadData Loop";
                    ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);
                }

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
                if (!PipeReader.TryRead(out ReadResult))
                {
                    LastAction = "Reading Data Asynchronously in ReadData Loop";
                    ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);
                }

                await PipeReader.AdvanceToLineTerminator(ReadResult, CancellationToken);
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

        protected ValueTask<ReadOnlySequence<byte>> ReadLine()
        {
            LastAction = "Finding End of Line Position";

            if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
            {
                return default;
            }

#if !NETSTANDARD2_0 && !NETCOREAPP2_2
            var reader = new SequenceReader<byte>(ReadResult.Buffer);

            if (reader.TryReadTo(out ReadOnlySequence<byte> line, (byte) '\n', false))
            {
                return new ValueTask<ReadOnlySequence<byte>>(line);
            }
#endif

            return ReadLineAsync();
        }

        private async ValueTask<ReadOnlySequence<byte>> ReadLineAsync()
        {
            var endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            if (endOfLinePosition == null)
            {
                LastAction = "Reading until End of Line found";

                ReadResult = await PipeReader.ReadUntilEndOfLineFound(ReadResult, CancellationToken);

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

    public class GenericResultProcessor : ResultProcessor<RawResult>
    {
        internal override async ValueTask<RawResult> Process()
        {
            var line = ReadResult.Buffer;

            if (line.IsEmpty)
            {
                return default;
            }

            var firstChar = line.ItemAt(0);

            // TODO Remove - Is for debugging
            var stringline = line.AsString();

            object result;

            if (firstChar == '*')
            {
                var processor = RedisClient.ArrayResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == '+')
            {
                var processor = RedisClient.WordResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ':')
            {
                var processor = RedisClient.IntegerResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == '$')
            {
                var processor = RedisClient.DataResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == '-')
            {
                var redisResponse = ReadResult.Buffer.AsString();
                PipeReader.AdvanceTo(ReadResult.Buffer.End);
                throw new RedisFailedCommandException(redisResponse, RedisClient.LastCommand);
            }
            else
            {
                var processor = RedisClient.EmptyResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }

            return new RawResult(result);
        }
    }

    public class EmptyResultProcessor : ResultProcessor<object>
    {
        internal override ValueTask<object> Process()
        {
            // Do Nothing!
            return default;
        }
    }

    public class SuccessResultProcessor : ResultProcessor<object>
    {
        internal override async ValueTask<object> Process()
        {
            var line = await ReadLine();

            if (line.Length < 3 ||
                line.ItemAt(0) != '+' ||
                line.ItemAt(1) != 'O' ||
                line.ItemAt(2) != 'K')
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                throw new RedisFailedCommandException(stringLine, LastCommand);
            }

            PipeReader.AdvanceTo(line.End);

            return null;
        }
    }

    public class DataResultProcessor : ResultProcessor<string>
    {
        internal override async ValueTask<string> Process()
        {
            return (await ReadData()).AsString();
        }
    }

    public class WordResultProcessor : ResultProcessor<string>
    {
        internal override async ValueTask<string> Process()
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
        internal override async ValueTask<int> Process()
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
        internal override async ValueTask<float> Process()
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
        internal override async ValueTask<IEnumerable<StringRedisValue>> Process()
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
                if (!PipeReader.TryRead(out ReadResult))
                {
                    LastAction = "Reading Data Asynchronously in ExpectArray";
                    ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);
                }

                results[i] = (await ReadData()).ToArray();
            }

            return results.ToRedisValues();
        }
    }
}