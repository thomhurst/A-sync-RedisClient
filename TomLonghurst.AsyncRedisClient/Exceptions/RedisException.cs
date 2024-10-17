using System.Text;

namespace TomLonghurst.AsyncRedisClient.Exceptions;

public abstract class RedisException : Exception
{
    protected static string ToString(byte[]? lastCommand)
    {
        if (lastCommand is null)
        {
            return "null";
        }

        if (lastCommand.Length == 0)
        {
            return string.Empty;
        }
        
        return Encoding.UTF8.GetString(lastCommand);
    }
}