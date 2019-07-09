namespace TomLonghurst.RedisClient.Exceptions
{
    public class UnexpectedRedisResponseException : RedisException
    {
        public override string Message { get; }

        public UnexpectedRedisResponseException(string message)
        {
            Message = message;
        }
    }
}