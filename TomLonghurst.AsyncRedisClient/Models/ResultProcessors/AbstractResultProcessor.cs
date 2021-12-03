using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public abstract class AbstractResultProcessor
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
            CancellationToken = cancellationToken;
        }

        internal void SetMembers(Client.RedisClient redisClient, PipeReader pipeReader, ReadResult readResult,
            CancellationToken cancellationToken)
        {
            RedisClient = redisClient;
            PipeReader = pipeReader;
            ReadResult = readResult;
            CancellationToken = cancellationToken;
        }
    }

    public abstract class AbstractResultProcessor<T> : AbstractResultProcessor
    {
        public string LastCommand
        {
            get => RedisClient.LastCommand;
            set => RedisClient.LastCommand = value;
        }

        internal async ValueTask<T> Start(Client.RedisClient redisClient, PipeReader pipeReader,
            CancellationToken cancellationToken)
        {
            SetMembers(redisClient, pipeReader, cancellationToken);

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

            var endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            if (endOfLinePosition != null)
            {
                return ReadResult.Buffer.Slice(ReadResult.Buffer.Start, endOfLinePosition.Value);
            }

            if (ReadResult.IsCompleted && ReadResult.Buffer.IsEmpty)
            {
                throw new RedisDataException("Read is completed and buffer is empty - Can't find a complete line in ReadLine'");
            }

            return await ReadLineAsync();
        }

        private async ValueTask<ReadOnlySequence<byte>> ReadLineAsync()
        {
            var endOfLinePosition = ReadResult.Buffer.GetEndOfLinePosition();
            if (endOfLinePosition == null)
            {

                ReadResult = await PipeReader.ReadUntilEndOfLineFound(ReadResult, CancellationToken);

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
}