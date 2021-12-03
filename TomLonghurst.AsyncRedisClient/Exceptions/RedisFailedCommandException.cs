namespace TomLonghurst.AsyncRedisClient.Exceptions
{
    public class RedisFailedCommandException : RedisRecoverableException
    {
        public RedisFailedCommandException(string message, string lastCommand)
        {
            Message = $"{message}\nLast Command: {lastCommand}";
        }

        public override string Message { get; }
    }
}