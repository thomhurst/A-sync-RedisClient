using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using StackExchange.Redis;
using Testcontainers.Redis;
using TomLonghurst.AsyncRedisClient.Client;

namespace Benchmark;

[MarkdownExporterAttribute.GitHub]
[SimpleJob(RuntimeMoniker.Net90)]
public class BenchmarkBase
{
    public RedisContainer RedisContainer { get; private set; } = null!;

    public RedisClient AsyncRedisClient { get; private set; } = null!;
    public IDatabaseAsync StackExchangeClient { get; private set; } = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        RedisContainer =  new RedisBuilder()
            .WithImage("redis:7.4.1")
            .Build();
        
        await RedisContainer.StartAsync();

        var connectionString = new Uri($"https://{RedisContainer.GetConnectionString()}");
        
        var redisManager = await RedisClientManager.ConnectAsync(new RedisClientConfig(connectionString.Host, connectionString.Port)
        {
            Ssl = false
        }, 
            1);
        AsyncRedisClient = redisManager.GetRedisClient();

        var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(RedisContainer.GetConnectionString());

        StackExchangeClient = connectionMultiplexer.GetDatabase();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await RedisContainer.DisposeAsync();
    }
}
