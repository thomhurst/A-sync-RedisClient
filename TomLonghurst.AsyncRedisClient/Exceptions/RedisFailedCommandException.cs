namespace TomLonghurst.AsyncRedisClient.Exceptions;

public class RedisFailedCommandException : RedisRecoverableException
{
    public RedisFailedCommandException(string message, byte[]? lastCommand)
    {
        Message = $"{message}\nLast Command: {ToString(lastCommand)}";
    }

    public override string Message { get; }
}