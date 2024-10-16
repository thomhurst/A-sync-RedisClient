namespace TomLonghurst.AsyncRedisClient;

internal class Logger
{
    internal void Info(string message)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Info)
        {
            log(message);
        }
    }

    internal void Debug(string message)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Debug)
        {
            log(message);
        }
    }

    internal void Error(string message)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Error)
        {
            log(message);
        }
    }

    internal void Error(string message, Exception ex)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Info)
        {
            log(message);
            log(ex.ToString());
        }
    }

    private void log(string message)
    {
        Console.WriteLine($"RedisClient ----- {message}");
    }
}