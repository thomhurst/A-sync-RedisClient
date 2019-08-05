namespace TomLonghurst.RedisClient.Exceptions
{
    public class RedisDataException : RedisException
    {
        public override string Message { get; }

        public RedisDataException(string message)
        {
            Message = message;
        }
    }
}