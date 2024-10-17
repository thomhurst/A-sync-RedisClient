using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Exceptions;

public class RedisWaitTimeoutException : RedisRecoverableException
{
    private readonly RedisClient _redisClient;

    internal RedisWaitTimeoutException(RedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    public override string Message
    {
        get
        {
            ApplicationStats.GetThreadPoolStats(out var ioThreadStats, out var workerThreadStats);
            return $"""
                    Client {_redisClient.ClientId}
                    {workerThreadStats}
                    {ioThreadStats}
                    Last Command: {ToString(_redisClient.LastCommand)}
                    """;
        }
    }
}