namespace TomLonghurst.AsyncRedisClient;

internal class Logger
{
    internal void Info(string message)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Info)
        {
            Log(message);
        }
    }

    internal void Debug(string message)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Debug)
        {
            Log(message);
        }
    }

    internal void Error(string message)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Error)
        {
            Log(message);
        }
    }

    internal void Error(string message, Exception ex)
    {
        if (RedisClientSettings.LogLevel >= LogLevel.Info)
        {
            Log(message);
            Log(ex.ToString());
        }
    }

    private void Log(string message)
    {
        Console.WriteLine($"RedisClient ----- {message}");
    }
}