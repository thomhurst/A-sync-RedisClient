namespace TomLonghurst.RedisClient.Exceptions
{
    public class RedisFailedCommandException : RedisException
    {
        public RedisFailedCommandException(string message, string lastCommand)
        {
            Message = $"{message}\nLast Command: {lastCommand}";
        }

        public override string Message { get; }
    }
}