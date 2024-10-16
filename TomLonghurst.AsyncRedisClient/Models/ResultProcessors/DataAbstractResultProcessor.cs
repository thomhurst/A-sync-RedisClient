using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class DataResultProcessor : AbstractResultProcessor<string>
{
    internal override async ValueTask<string> Process()
    {
        return (await ReadData()).AsString();
    }
}