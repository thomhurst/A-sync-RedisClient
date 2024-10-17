using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class FloatResultProcessor : AbstractResultProcessor<float>
{
    internal override async ValueTask<float> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var floatString = (await ReadData(redisClient, pipeReader, readResult, cancellationToken)).AsString();

        if (!float.TryParse(floatString, out var number))
        {
            throw new UnexpectedRedisResponseException(floatString);
        }

        return number;
    }
}