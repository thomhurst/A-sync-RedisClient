using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class WordResultProcessor : AbstractResultProcessor<string>
    {
        internal override async ValueTask<string> Process()
        {
            var line = await ReadLine();

            if (line.ItemAt(0) != ByteConstants.Plus)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                
                PipeReader.AdvanceTo(line.End);
                
                if (line.ItemAt(0) == ByteConstants.Dash)
                {
                    throw new RedisFailedCommandException(stringLine, LastCommand);
                }
                
                throw new UnexpectedRedisResponseException(stringLine);
            }

            var word = line.Slice(line.GetPosition(1, line.Start)).AsStringWithoutLineTerminators();
            PipeReader.AdvanceTo(line.End);
            return word;
        }
    }
}