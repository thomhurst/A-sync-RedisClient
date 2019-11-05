using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class IntegerAbstractAbstractResultProcessor : AbstractResultProcessor<int>
    {
        internal override async ValueTask<int> Process()
        {
            var line = await ReadLine();

            if (line.ItemAt(0) != ByteConstants.Colon)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                
                if (line.ItemAt(0) == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }
                
                throw new UnexpectedRedisResponseException(stringLine);
                
            }

            var number = SpanNumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

            if (number == -1)
            {
                return -1;
            }

            return (int) number;
        }
    }
}