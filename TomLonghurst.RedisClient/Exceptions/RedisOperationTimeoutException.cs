using TomLonghurst.RedisClient.Helpers;

namespace TomLonghurst.RedisClient.Exceptions
{
    public class RedisOperationTimeoutException : RedisRecoverableException
    {
        private readonly Client.RedisClient _redisClient;

        internal RedisOperationTimeoutException(Client.RedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        public override string Message
        {
            get
            {
                ApplicationStats.GetThreadPoolStats(out var ioThreadStats, out var workerThreadStats);
                return $"Client {_redisClient.ClientId}\n{workerThreadStats}\n{ioThreadStats}\nLast Command: {_redisClient.LastCommand}\nLast Action: {_redisClient.LastAction}";
            }
        }
    }
}