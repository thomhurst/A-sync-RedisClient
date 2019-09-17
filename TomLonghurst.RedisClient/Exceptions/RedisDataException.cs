namespace TomLonghurst.RedisClient.Exceptions
{
    public class RedisDataException : RedisNonRecoverableException
    {
        public override string Message { get; }

        public RedisDataException(string message)
        {
            Message = message;
        }
    }
}