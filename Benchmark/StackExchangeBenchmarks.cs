using BenchmarkDotNet.Attributes;

namespace Benchmark;

public class StackExchangeBenchmarks : StackExchangeClientBase
{
    [Benchmark]
    public async Task Redis()
    {
        await Client.StringSetAsync("MyKey", "MyValue");
    }
}
