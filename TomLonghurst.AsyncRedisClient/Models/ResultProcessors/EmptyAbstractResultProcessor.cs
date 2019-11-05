using System.Threading.Tasks;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class EmptyResultProcessor : AbstractResultProcessor<object>
    {
        internal override ValueTask<object> Process()
        {
            // Do Nothing!
            return new ValueTask<object>();
        }
    }
}