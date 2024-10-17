using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Testcontainers.Redis;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net90)]
public class BenchmarkBase
{
    protected RedisContainer RedisContainer { get; private set; } = null!;

    public async Task ContainerSetup()
    {
        RedisContainer =  new RedisBuilder()
            .WithImage("redis:7.4.1")
            .Build();
        
        await RedisContainer.StartAsync();
    }

    public async Task ContainerCleanup()
    {
        await RedisContainer.DisposeAsync();
    }
}
