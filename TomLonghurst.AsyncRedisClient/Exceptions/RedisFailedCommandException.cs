using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Exceptions
{
    public class RedisFailedCommandException : RedisException
    {
        public RedisFailedCommandException(string message, IRedisCommand lastCommand)
        {
            Message = $"{message}\nLast Command: {lastCommand}";
        }

        public override string Message { get; }
    }
}