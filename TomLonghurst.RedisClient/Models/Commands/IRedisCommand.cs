namespace TomLonghurst.RedisClient.Models.Commands
{
    public interface IRedisCommand
    {
        byte[] EncodedCommand { get; }
        string AsString { get; }
    }
}