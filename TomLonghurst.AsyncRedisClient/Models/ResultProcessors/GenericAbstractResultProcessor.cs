using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class GenericResultProcessor : AbstractResultProcessor<RawResult>
{
    internal override async ValueTask<RawResult> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var firstChar = await ReadByte(pipeReader, cancellationToken);

        pipeReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.Slice(1).Start);
            
        if (firstChar == ByteConstants.Dash)
        {
            var line = await ReadLine(pipeReader, cancellationToken);
            var redisResponse = line.AsString();
            pipeReader.AdvanceTo(line.End);
            throw new RedisFailedCommandException(redisResponse, redisClient.LastCommand);
        }

        var result = ProcessData(redisClient, pipeReader, readResult, firstChar, cancellationToken);

        return new RawResult(result);
    }

    private async ValueTask<object?> ProcessData(RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        byte firstChar,
        CancellationToken cancellationToken)
    {
        if (firstChar == ByteConstants.Asterix)
        {
            return await redisClient.ArrayResultProcessor.Process(redisClient, pipeReader, readResult, cancellationToken);
        }

        if (firstChar == ByteConstants.Plus)
        {
            return await redisClient.WordResultProcessor.Process(redisClient, pipeReader, readResult, cancellationToken);
        }
        
        if (firstChar == ByteConstants.Colon)
        {
            return await redisClient.IntegerResultProcessor.Process(redisClient, pipeReader, readResult, cancellationToken);
        }
        
        if (firstChar == ByteConstants.Dollar)
        {
            return await redisClient.DataResultProcessor.Process(redisClient, pipeReader, readResult, cancellationToken);
        }
        
        return await redisClient.EmptyResultProcessor.Process(redisClient, pipeReader, readResult, cancellationToken);
    }
}