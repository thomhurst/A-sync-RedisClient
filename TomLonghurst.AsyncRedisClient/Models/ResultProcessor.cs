using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
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

            LastAction = "Starting ResultProcessor.Processor";
            return await Process();
        }

        internal abstract ValueTask<T> Process();


        protected async ValueTask<Memory<byte>> ReadData()
        {
            var line = await ReadLine();

            if (line.IsEmpty)
            {
                throw new RedisDataException("Empty buffer at start of ReadData");
            }

            var firstChar = line.ItemAt(0);

            if (firstChar != ByteConstants.Dollar)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);

                if (firstChar == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }

                throw new UnexpectedRedisResponseException($"Unexpected reply: {stringLine}");
            }

            var alreadyReadToLineTerminator = false;

            var byteSizeOfData = SpanNumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

            if (byteSizeOfData == -1)
            {
                return null;
            }

            if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
            {
                throw new RedisDataException("ReadResult is completed and buffer is empty starting ReadData");
            }

            LastAction = "Reading Data in ReadData";
            ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);

            var buffer = ReadResult.Buffer;

            if (byteSizeOfData == 0)
            {
                throw new UnexpectedRedisResponseException("Invalid length");
            }

            var dataByteStorage = new byte[byteSizeOfData].AsMemory();

            buffer = buffer.Slice(buffer.Start, Math.Min(byteSizeOfData, buffer.Length));

            var bytesReceived = buffer.Length;

            buffer.CopyTo(dataByteStorage.Slice(0, (int) bytesReceived).Span);

            LastAction = "Advancing Buffer in ReadData Loop";
            if (bytesReceived >= byteSizeOfData)
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
                    throw new RedisDataException(
                        "ReadResult is completed and buffer is empty reading in loop in ReadData");
                }

                if (ReadResult.IsCanceled)
                {
                    throw new RedisDataException("ReadResult is cancelled reading in loop in ReadData");
                }

                LastAction = "Reading Data in ReadData Loop";
                ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);

                buffer = ReadResult.Buffer.Slice(ReadResult.Buffer.Start,
                    Math.Min(ReadResult.Buffer.Length, byteSizeOfData - bytesReceived));

                buffer
                    .CopyTo(dataByteStorage.Slice((int) bytesReceived,
                        (int) Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).Span);

                bytesReceived += buffer.Length;

                if (bytesReceived >= byteSizeOfData)
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
                LastAction = "Reading Data in ReadData Loop";
                ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);

                await PipeReader.AdvanceToLineTerminator(ReadResult, CancellationToken);
            }

            return dataByteStorage;
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

        protected async ValueTask<byte> ReadByte()
        {
            ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken);

            if (ReadResult.Buffer.IsEmpty)
            {
                throw new RedisDataException("Empty buffer in ReadByte");
            }

            return ReadResult.Buffer.Slice(ReadResult.Buffer.Start, 1).First.Span[0];
        }

        protected async ValueTask<ReadOnlySequence<byte>> ReadLine()
        {
            ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken);
         
            LastAction = "Finding End of Line Position";
            var endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            if (endOfLinePosition != null)
            {
                return ReadResult.Buffer.Slice(ReadResult.Buffer.Start, endOfLinePosition.Value);
            }

            if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
            {
                throw new Exception("Read is completed and buffer is empty - Can't find a complete line in ReadLine'");
            }

            return await ReadLineAsync();
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

            return buffer.Slice(buffer.Start, endOfLinePosition.Value);
        }
    }

    public class GenericResultProcessor : ResultProcessor<RawResult>
    {
        internal override async ValueTask<RawResult> Process()
        {
            var firstChar = await ReadByte();

            if (firstChar == ByteConstants.Dash)
            {
                PipeReader.AdvanceTo(ReadResult.Buffer.Start, ReadResult.Buffer.Slice(1).Start);
                var line = await ReadLine();
                var redisResponse = line.AsString();
                PipeReader.AdvanceTo(line.End);
                throw new RedisFailedCommandException(redisResponse, RedisClient.LastCommand);
            }

            PipeReader.AdvanceTo(ReadResult.Buffer.Start, ReadResult.Buffer.Slice(1).Start);
            
            object result;

            if (firstChar == ByteConstants.Asterix)
            {
                var processor = RedisClient.ArrayResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Plus)
            {
                var processor = RedisClient.WordResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Colon)
            {
                var processor = RedisClient.IntegerResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Dollar)
            {
                var processor = RedisClient.DataResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
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
            return new ValueTask<object>();
        }
    }

    public class SuccessResultProcessor : ResultProcessor<object>
    {
        internal override async ValueTask<object> Process()
        {
            var line = await ReadLine();

            if (line.Length < 3 ||
                line.ItemAt(0) != ByteConstants.Plus ||
                line.ItemAt(1) != ByteConstants.O ||
                line.ItemAt(2) != ByteConstants.K)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                
                if (line.ItemAt(0) == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }
                
                throw new UnexpectedRedisResponseException(stringLine);
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
            var line = await ReadLine();

            if (line.ItemAt(0) != ByteConstants.Plus)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                
                PipeReader.AdvanceTo(line.End);
                
                if (line.ItemAt(0) == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }
                
                throw new UnexpectedRedisResponseException(stringLine);
            }

            var word = line.Slice(line.GetPosition(1, line.Start)).AsStringWithoutLineTerminators();
            PipeReader.AdvanceTo(line.End);
            return word;
        }
    }

    public class IntegerResultProcessor : ResultProcessor<int>
    {
        internal override async ValueTask<int> Process()
        {
            var line = await ReadLine();

            if (line.ItemAt(0) != ByteConstants.Colon)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                
                if (line.ItemAt(0) == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }
                
                throw new UnexpectedRedisResponseException(stringLine);
                
            }

            var number = SpanNumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

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
            var line = await ReadLine();

            if (line.ItemAt(0) != ByteConstants.Asterix)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                throw new UnexpectedRedisResponseException(stringLine);
            }

            var count = SpanNumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

            var results = new byte [count][];
            for (var i = 0; i < count; i++)
            {
                // Refresh the pipe buffer before 'ReadData' method reads it
                LastAction = "Reading Data in ExpectArray";
                ReadResult = await PipeReader.ReadAsyncOrThrowReadTimeout(CancellationToken).ConfigureAwait(false);

                results[i] = (await ReadData()).ToArray();
            }

            return results.ToRedisValues();
        }
    }
}