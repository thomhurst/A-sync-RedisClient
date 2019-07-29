using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Exceptions
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