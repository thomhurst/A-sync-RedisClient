using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Models
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

            if (!PipeReader.TryRead(out ReadResult))
            {
                ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);
            }

            return await Process();
        }

        private protected abstract ValueTask<T> Process();

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async ValueTask<Memory<byte>> ReadData()
        {
            var buffer = ReadResult.Buffer;

            if (buffer.IsEmpty && ReadResult.IsCompleted)
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var line = await ReadLine();

            var firstChar = line.First.Span[0];

            if (firstChar == '-')
            {
                throw new RedisFailedCommandException(await ReadLineAsString(), LastCommand);
            }

            if (firstChar == '$')
            {
                if (line.Length >= 3 && line.ItemAt(1) == '-' && line.ItemAt(2) == '1')
                {
                    PipeReader.AdvanceTo(line.End);
                    return null;
                }

                var lineAsString = await ReadLineAsString();

                LastAction = "Reading Data Synchronously in ReadData";
                if (!PipeReader.TryRead(out ReadResult))
                {
                    LastAction = "Reading Data Asynchronously in ReadData";
                    ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);
                }

                buffer = ReadResult.Buffer;
                
                if (long.TryParse(lineAsString.Substring(1), out var byteSizeOfData))
                {
                    var bytes = new byte[byteSizeOfData].AsMemory();

                    buffer = buffer.Slice(0, Math.Min(byteSizeOfData, buffer.Length));

                    var bytesReceived = buffer.Length;

                    buffer.CopyTo(bytes.Slice(0, (int) bytesReceived).Span);

                    PipeReader.AdvanceTo(buffer.End);

                    while (bytesReceived < byteSizeOfData)
                    {
                        LastAction = "Advancing Buffer in ReadData Loop";

                        if ((ReadResult.IsCompleted || ReadResult.IsCanceled) && ReadResult.Buffer.IsEmpty)
                        {
                            break;
                        }

                        LastAction = "Reading Data Synchronously in ReadData Loop";
                        if (!PipeReader.TryRead(out ReadResult))
                        {
                            LastAction = "Reading Data Asynchronously in ReadData Loop";
                            ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);
                        }

                        buffer = ReadResult.Buffer.Slice(0,
                            Math.Min(ReadResult.Buffer.Length, byteSizeOfData - bytesReceived));

                        buffer
                            .CopyTo(bytes.Slice((int) bytesReceived,
                                (int) Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).Span);

                        bytesReceived += buffer.Length;

                        PipeReader.AdvanceTo(buffer.End);
                    }

                    if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
                    {
                        return bytes;
                    }

                    if (!PipeReader.TryRead(out ReadResult))
                    {
                        LastAction = "Reading Data Asynchronously in ReadData Loop";
                        ReadResult = await PipeReader.ReadAsync().ConfigureAwait(false);
                    }

                    await PipeReader.AdvanceToLineTerminator(ReadResult);

                    return bytes;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }

        protected async Task<ReadOnlySequence<byte>> ReadLine()
        {
            LastAction = "Finding End of Line Position";
            var endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            if (endOfLinePosition == null)
            {
                LastAction = "Reading until End of Line found";
                var readResultWithEndOfLine = await PipeReader.ReadUntilEndOfLineFound(ReadResult);
                ReadResult = readResultWithEndOfLine.ReadResult;

                LastAction = "Finding End of Line Position";
                endOfLinePosition = readResultWithEndOfLine.EndOfLinePosition;
            }

            if (endOfLinePosition == null)
            {
                throw new RedisDataException("Can't find EOL");
            }

            var buffer = ReadResult.Buffer;

            return buffer.Slice(0, endOfLinePosition.Value);
        }

        protected async ValueTask<string> ReadLineAsString()
        {
            var buffer = await ReadLine();

            // Reslice but removing the line terminators
            var line = buffer.Slice(0, buffer.Length - 2).AsString();

            LastAction = "Advancing Buffer to End of Line";
            PipeReader.AdvanceTo(buffer.End);

            return line;
        }
    }

    public class SuccessResultProcessor : ResultProcessor<object>
    {
        private protected override async ValueTask<object> Process()
        {
            var buffer = await ReadLine();
            if(buffer.First.Span[0] == '-')
            {
                
                throw new RedisFailedCommandException(await ReadLineAsString(), LastCommand);
            }
            
            PipeReader.AdvanceTo(buffer.End);
            
            return new object();
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
            var word = await ReadLineAsString();

            if (!word.StartsWith("+"))
            {
                throw new UnexpectedRedisResponseException(word);
            }

            return word.Substring(1);
        }
    }

    public class IntegerResultProcessor : ResultProcessor<int>
    {
        private protected override async ValueTask<int> Process()
        {
            var line = await ReadLineAsString();

            if (!line.StartsWith(":") || !int.TryParse(line.Substring(1), out var number))
            {
                throw new UnexpectedRedisResponseException(line);
            }

            return number;
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
            var arrayWithCountLine = await ReadLineAsString();

            if (!arrayWithCountLine.StartsWith("*"))
            {
                throw new UnexpectedRedisResponseException(arrayWithCountLine);
            }

            if (!int.TryParse(arrayWithCountLine.Substring(1), out var count))
            {
                throw new UnexpectedRedisResponseException($"Error getting message count: {arrayWithCountLine}");
            }

            var results = new byte [count][];
            for (var i = 0; i < count; i++)
            {
                // Refresh the pipe buffer before 'ReadData' method reads it
                LastAction = "Reading Data Synchronously in ExpectArray";
                if (!PipeReader.TryRead(out ReadResult))
                {
                    LastAction = "Reading Data Asynchronously in ExpectArray";
                    var readPipeTask = PipeReader.ReadAsync();
                    ReadResult = await readPipeTask.ConfigureAwait(false);
                }

                results[i] = (await ReadData()).ToArray();
            }

            return results.ToRedisValues();
        }
    }
}