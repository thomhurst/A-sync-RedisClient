using System.Text;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class IntegerResultProcessor : AbstractResultProcessor<int>
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
                    throw new RedisFailedCommandException(stringLine, Encoding.UTF8.GetString(LastCommand.ToArray()));
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