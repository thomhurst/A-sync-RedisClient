using System.Collections.Generic;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class ArrayResultProcessor : AbstractResultProcessor<IEnumerable<string>>
    {
        internal override async ValueTask<IEnumerable<string>> Process()
        {
            var line = await ReadLine();

            if (line.ItemAt(0) != ByteConstants.Asterix)
            {
                var stringLine = line.AsStringWithoutLineTerminators();
                PipeReader.AdvanceTo(line.End);
                throw new UnexpectedRedisResponseException(stringLine);
            }

            var count = SpanNumberParser.Parse(line);

            PipeReader.AdvanceTo(line.End);

            var results = new byte [count][];
            for (var i = 0; i < count; i++)
            {
                results[i] = (await ReadData()).ToArray();
            }

            return results.ToStringsFromUtf8();
        }
    }
}