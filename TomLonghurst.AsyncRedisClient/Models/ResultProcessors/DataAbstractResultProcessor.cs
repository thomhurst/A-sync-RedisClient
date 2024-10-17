using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class DataResultProcessor : AbstractResultProcessor<string>
{
    internal override async ValueTask<string> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        return (await ReadData(redisClient, pipeReader, readResult, cancellationToken)).AsString();
    }
}