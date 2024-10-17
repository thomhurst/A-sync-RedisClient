using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class EmptyResultProcessor : AbstractResultProcessor<object?>
{
    internal override ValueTask<object?> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        // Do Nothing!
        return ValueTask.FromResult<object?>(null);
    }
}