using BenchmarkDotNet.Attributes;

namespace Benchmark;

public class AsyncRedisBenchmarks : AsyncRedisClientBase
{
    [Benchmark]
    public async Task Redis()
    {
        await Client.StringSetAsync("MyKey", "MyValue");
    }
}
