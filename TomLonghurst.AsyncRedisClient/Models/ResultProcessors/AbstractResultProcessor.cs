using System.Buffers;
using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public abstract class AbstractResultProcessor
{
}

public abstract class AbstractResultProcessor<T> : AbstractResultProcessor
{
    internal async ValueTask<T> Start(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        return await Process(redisClient, pipeReader, readResult, cancellationToken);
    }

    internal abstract ValueTask<T> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    );


    protected async ValueTask<Memory<byte>> ReadData(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
        )
    {
        var line = await ReadLine(pipeReader, cancellationToken);

        if (line.IsEmpty)
        {
            throw new RedisDataException("Empty buffer at start of ReadData");
        }

        var firstChar = line.ItemAt(0);

        if (firstChar != ByteConstants.Dollar)
        {
            var stringLine = line.AsStringWithoutLineTerminators() ?? string.Empty;
            pipeReader.AdvanceTo(line.End);

            if (firstChar == ByteConstants.Dash)
            {
                throw new RedisFailedCommandException(stringLine, redisClient.LastCommand);
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {stringLine}");
        }

        var alreadyReadToLineTerminator = false;

        var byteSizeOfData = SpanNumberParser.Parse(line);

        pipeReader.AdvanceTo(line.End);

        if (byteSizeOfData == -1)
        {
            return null;
        }

        if (readResult is { IsCompleted: true, Buffer.IsEmpty: true })
        {
            throw new RedisDataException("ReadResult is completed and buffer is empty starting ReadData");
        }

        readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken);

        var buffer = readResult.Buffer;

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
            alreadyReadToLineTerminator = TryAdvanceToLineTerminator(ref buffer, readResult, pipeReader);
        }
        else
        {
            pipeReader.AdvanceTo(buffer.End);
        }

        while (bytesReceived < byteSizeOfData)
        {
            if (readResult is { IsCompleted: true, Buffer.IsEmpty: true })
            {
                throw new RedisDataException(
                    "ReadResult is completed and buffer is empty reading in loop in ReadData");
            }

            if (readResult.IsCanceled)
            {
                throw new RedisDataException("ReadResult is cancelled reading in loop in ReadData");
            }

            readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken);

            buffer = readResult.Buffer.Slice(readResult.Buffer.Start,
                Math.Min(readResult.Buffer.Length, byteSizeOfData - bytesReceived));

            buffer
                .CopyTo(dataByteStorage.Slice((int) bytesReceived,
                    (int) Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).Span);

            bytesReceived += buffer.Length;

            if (bytesReceived >= byteSizeOfData)
            {
                alreadyReadToLineTerminator = TryAdvanceToLineTerminator(ref buffer, readResult, pipeReader);
            }
            else
            {
                pipeReader.AdvanceTo(buffer.End);
            }
        }

        if (!alreadyReadToLineTerminator)
        {
            readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken);

            await pipeReader.AdvanceToLineTerminator(readResult, cancellationToken);
        }

        return dataByteStorage;
    }

    private bool TryAdvanceToLineTerminator(ref ReadOnlySequence<byte> buffer, ReadResult readResult, PipeReader pipeReader)
    {
        var slicedBytes = readResult.Buffer.Slice(buffer.End);
        if (slicedBytes.IsEmpty)
        {
            pipeReader.AdvanceTo(buffer.End);
            return false;
        }

        var endOfLinePosition = slicedBytes.GetEndOfLinePosition();
        if (endOfLinePosition == null)
        {
            pipeReader.AdvanceTo(buffer.End);
            return false;
        }
            
        pipeReader.AdvanceTo(endOfLinePosition.Value);
        return true;
    }

    protected async ValueTask<byte> ReadByte(PipeReader pipeReader, CancellationToken cancellationToken)
    {
        var readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken);

        if (readResult.Buffer.IsEmpty)
        {
            throw new RedisDataException("Empty buffer in ReadByte");
        }

        return readResult.Buffer.Slice(readResult.Buffer.Start, 1).First.Span[0];
    }

    protected async ValueTask<ReadOnlySequence<byte>> ReadLine(
        PipeReader pipeReader, 
        CancellationToken cancellationToken
        )
    {
        var readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken);

        var endOfLinePosition = readResult.Buffer.GetEndOfLinePosition();
        if (endOfLinePosition != null)
        {
            return readResult.Buffer.Slice(readResult.Buffer.Start, endOfLinePosition.Value);
        }

        if (readResult is { IsCompleted: true, Buffer.IsEmpty: true })
        {
            throw new RedisDataException("Read is completed and buffer is empty - Can't find a complete line in ReadLine'");
        }

        return await ReadLineAsync(pipeReader, readResult, cancellationToken);
    }

    private async ValueTask<ReadOnlySequence<byte>> ReadLineAsync(
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var endOfLinePosition = readResult.Buffer.GetEndOfLinePosition();
        if (endOfLinePosition == null)
        {
            readResult = await pipeReader.ReadUntilEndOfLineFound(readResult, cancellationToken);
            
            endOfLinePosition = readResult.Buffer.GetEndOfLinePosition();
        }

        if (endOfLinePosition == null)
        {
            throw new RedisDataException("Can't find EOL in ReadLine");
        }

        var buffer = readResult.Buffer;

        return buffer.Slice(buffer.Start, endOfLinePosition.Value);
    }
}