namespace TomLonghurst.AsyncRedisClient.Exceptions;

public class UnexpectedRedisResponseException : RedisNonRecoverableException
{
    public override string Message { get; }

    public UnexpectedRedisResponseException(string message)
    {
        Message = message;
    }
}