using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TomLonghurst.AsyncRedisClient.Client;

namespace Benchmark;

[MarkdownExporterAttribute.GitHub]
[SimpleJob(RuntimeMoniker.Net90)]
public class AsyncRedisClientBase : BenchmarkBase
{
    private RedisClientManager _redisClientManager = null!;

    public RedisClient Client { get; private set; } = null!;

    [IterationSetup]
    public async Task Setup()
    {
        var connectionString = new Uri($"https://{RedisContainer.GetConnectionString()}");
        
        _redisClientManager = await RedisClientManager.ConnectAsync(new RedisClientConfig(connectionString.Host, connectionString.Port)
        {
            Ssl = false
        });

        Client = _redisClientManager.GetRedisClient();
    }

    [IterationCleanup]
    public async Task Cleanup()
    {
        await RedisContainer.DisposeAsync();
        await _redisClientManager.DisposeAsync();
    }
}
