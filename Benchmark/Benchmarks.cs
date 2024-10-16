using BenchmarkDotNet.Attributes;
using TomLonghurst.AsyncRedisClient.Enums;

namespace Benchmark;

public class Benchmarks : BenchmarkBase
{
    [Benchmark]
    public async Task AsyncRedis()
    {
        await AsyncRedisClient.StringSetAsync("MyKey", "MyValue", AwaitOptions.AwaitCompletion);
    }

    [Benchmark]
    public async Task StackExchange()
    {
        await StackExchangeClient.StringSetAsync("MyKey", "MyValue");
    }
}
