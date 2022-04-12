using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class FloatResultProcessor : AbstractResultProcessor<float>
    {
        internal override async ValueTask<float> Process()
        {
            var floatString = (await ReadData()).AsString();

            if (!float.TryParse(floatString, out var number))
            {
                throw new UnexpectedRedisResponseException(floatString);
            }

            return number;
        }
    }
}