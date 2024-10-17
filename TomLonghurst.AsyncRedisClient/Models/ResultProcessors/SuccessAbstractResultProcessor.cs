using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class SuccessResultProcessor : AbstractResultProcessor<object?>
{
    internal override async ValueTask<object?> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var line = await ReadLine(pipeReader, cancellationToken);

        if (line.Length < 3 ||
            line.ItemAt(0) != ByteConstants.Plus ||
            line.ItemAt(1) != ByteConstants.O ||
            line.ItemAt(2) != ByteConstants.K)
        {
            var stringLine = line.AsStringWithoutLineTerminators();
            pipeReader.AdvanceTo(line.End);
                
            if (stringLine[0] == ByteConstants.Dash)
            {
                throw new RedisFailedCommandException(stringLine, redisClient.LastCommand);
            }
                
            throw new UnexpectedRedisResponseException(stringLine);
        }

        pipeReader.AdvanceTo(line.End);

        return null;
    }
}