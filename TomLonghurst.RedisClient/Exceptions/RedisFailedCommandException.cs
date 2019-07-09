namespace TomLonghurst.RedisClient.Exceptions
{
    public class RedisFailedCommandException : RedisException
    {
        public RedisFailedCommandException(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }
}