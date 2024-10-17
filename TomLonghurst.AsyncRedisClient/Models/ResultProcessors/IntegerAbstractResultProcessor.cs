using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class IntegerResultProcessor : AbstractResultProcessor<int>
{
    internal override async ValueTask<int> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var line = await ReadLine(pipeReader, cancellationToken);

        if (line.ItemAt(0) != ByteConstants.Colon)
        {
            var stringLine = line.AsStringWithoutLineTerminators();
            pipeReader.AdvanceTo(line.End);
                
            if (line.ItemAt(0) == ByteConstants.Dash)
            {
                throw new RedisFailedCommandException(stringLine, redisClient.LastCommand);
            }
                
            throw new UnexpectedRedisResponseException(stringLine);
                
        }

        var number = SpanNumberParser.Parse(line);

        pipeReader.AdvanceTo(line.End);

        if (number == -1)
        {
            return -1;
        }

        return (int) number;
    }
}