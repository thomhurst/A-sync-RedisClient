using BenchmarkDotNet.Attributes;
using TomLonghurst.AsyncRedisClient.Client;

namespace Benchmark;

public class AsyncRedisClientBase : BenchmarkBase
{
    private RedisClientManager _redisClientManager = null!;

    public RedisClient Client { get; private set; } = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        await ContainerSetup();
        var connectionString = new Uri($"https://{RedisContainer.GetConnectionString()}");
        
        _redisClientManager = await RedisClientManager.ConnectAsync(new RedisClientConfig(connectionString.Host, connectionString.Port)
        {
            Ssl = false,
            PoolSize = 1
        });

        Client = _redisClientManager.GetRedisClient();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await RedisContainer.DisposeAsync();
        await _redisClientManager.DisposeAsync();
        await ContainerCleanup();
    }
}
