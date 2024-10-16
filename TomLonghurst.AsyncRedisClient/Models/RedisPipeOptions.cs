using System.IO.Pipelines;

namespace TomLonghurst.AsyncRedisClient.Models;

public record RedisPipeOptions
{
    public PipeOptions? SendOptions { get; init; }
    public PipeOptions? ReceiveOptions { get; init; }
}