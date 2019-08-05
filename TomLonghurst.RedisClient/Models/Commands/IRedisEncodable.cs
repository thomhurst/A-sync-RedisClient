namespace TomLonghurst.RedisClient.Models.Commands
{
    public interface IRedisEncodable
    {
        string AsString { get; }
        byte[] RedisEncodedBytes { get; }
    }
}