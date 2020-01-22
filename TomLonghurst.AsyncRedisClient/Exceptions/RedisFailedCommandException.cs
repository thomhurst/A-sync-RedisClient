using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Exceptions
{
    public class RedisFailedCommandException : RedisRecoverableException
    {
        public RedisFailedCommandException(string message, IRedisEncodable lastCommand)
        {
            Message = $"{message}\nLast Command: {lastCommand.AsString}";
        }

        public override string Message { get; }
    }
}