namespace TomLonghurst.RedisClient.Exceptions
{
    public class UnexpectedRedisResponseException : RedisRecoverableException
    {
        public override string Message { get; }

        public UnexpectedRedisResponseException(string message)
        {
            Message = message;
        }
    }
}