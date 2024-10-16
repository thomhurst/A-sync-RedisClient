namespace TomLonghurst.AsyncRedisClient.Exceptions;

public class RedisReadTimeoutException : RedisNonRecoverableException
{
    private readonly Exception _exception;

    public RedisReadTimeoutException(Exception exception)
    {
        _exception = exception;
    }

    public override Exception GetBaseException() => _exception;
}