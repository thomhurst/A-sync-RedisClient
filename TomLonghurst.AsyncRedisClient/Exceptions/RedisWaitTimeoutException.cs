using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Exceptions;

public class RedisWaitTimeoutException : RedisRecoverableException
{
    private readonly Client.RedisClient _redisClient;

    internal RedisWaitTimeoutException(Client.RedisClient redisClient)
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
                    Last Command: {_redisClient.LastCommand.AsString}
                    """;
        }
    }
}