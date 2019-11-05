using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class SuccessAbstractAbstractResultProcessor : AbstractResultProcessor<object>
    {
        internal override async ValueTask<object> Process()
        {
            var line = await ReadLine();

            if (line.Length < 3 ||
                line.ItemAt(0) != ByteConstants.Plus ||
                line.ItemAt(1) != ByteConstants.O ||
                line.ItemAt(2) != ByteConstants.K)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                
                if (stringLine[0] == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }
                
                throw new UnexpectedRedisResponseException(stringLine);
            }

            PipeReader.AdvanceTo(line.End);

            return null;
        }
    }
}