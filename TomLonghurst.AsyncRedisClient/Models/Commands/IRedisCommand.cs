namespace TomLonghurst.AsyncRedisClient.Models.Commands;

public interface IRedisCommand
{
    byte[][] EncodedCommandList { get; }
    string AsString { get; }
}