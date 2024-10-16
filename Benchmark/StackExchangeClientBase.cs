using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using StackExchange.Redis;

namespace Benchmark;

public class StackExchangeClientBase : BenchmarkBase
{
    private ConnectionMultiplexer _connectionMultiplexer = null!;

    public IDatabaseAsync Client { get; private set; } = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        await ContainerSetup();

        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(RedisContainer.GetConnectionString());

        Client = _connectionMultiplexer.GetDatabase();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await RedisContainer.DisposeAsync();
        await _connectionMultiplexer.DisposeAsync();
        await ContainerCleanup();
    }
}
