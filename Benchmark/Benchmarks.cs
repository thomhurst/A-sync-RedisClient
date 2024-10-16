using BenchmarkDotNet.Attributes;

namespace Benchmark;

public class Benchmarks : BenchmarkBase
{
    [Benchmark]
    public async Task AsyncRedis()
    {
        await AsyncRedisClient.StringSetAsync("MyKey", "MyValue");
    }

    [Benchmark]
    public async Task StackExchange()
    {
        await StackExchangeClient.StringSetAsync("MyKey", "MyValue");
    }
}
