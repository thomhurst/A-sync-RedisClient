namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public interface IRedisEncodable
    {
        string AsString { get; }
        byte[] GetEncodedCommand();
    }
}