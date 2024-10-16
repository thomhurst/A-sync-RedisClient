using BenchmarkDotNet.Attributes;

namespace Benchmark;

public class StackExchangeBenchmarks : StackExchangeClientBase
{
    [Benchmark]
    public async Task StackExchangeRedis()
    {
        await Client.StringSetAsync("MyKey", "MyValue");
    }
}
