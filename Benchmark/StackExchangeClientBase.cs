using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using StackExchange.Redis;

namespace Benchmark;

[MarkdownExporterAttribute.GitHub]
[SimpleJob(RuntimeMoniker.Net90)]
public class StackExchangeClientBase : BenchmarkBase
{
    private ConnectionMultiplexer _connectionMultiplexer = null!;

    public IDatabaseAsync Client { get; private set; } = null!;

    [IterationSetup]
    public async Task Setup()
    {
        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(RedisContainer.GetConnectionString());

        Client = _connectionMultiplexer.GetDatabase();
    }

    [IterationCleanup]
    public async Task Cleanup()
    {
        await RedisContainer.DisposeAsync();
        await _connectionMultiplexer.DisposeAsync();
    }
}
