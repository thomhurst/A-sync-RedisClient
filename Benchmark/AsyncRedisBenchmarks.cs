using BenchmarkDotNet.Attributes;

namespace Benchmark;

public class AsyncRedisBenchmarks : AsyncRedisClientBase
{
    [Benchmark]
    public async Task AsyncRedis()
    {
        await Client.StringSetAsync("MyKey", "MyValue");
    }
}
