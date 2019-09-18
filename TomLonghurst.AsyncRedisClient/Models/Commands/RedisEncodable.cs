using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public class RedisEncodable : IRedisEncodable
    {
        public string AsString { get; }
        public byte[] RedisEncodedBytes { get; }
        
        public RedisEncodable(string stringCommand)
        {
            AsString = stringCommand;
            RedisEncodedBytes = stringCommand.ToUtf8BytesWithTerminator();
        }
    }
}