using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class GenericAbstractAbstractResultProcessor : AbstractResultProcessor<RawResult>
    {
        internal override async ValueTask<RawResult> Process()
        {
            var firstChar = await ReadByte();

            PipeReader.AdvanceTo(ReadResult.Buffer.Start, ReadResult.Buffer.Slice(1).Start);
            
            if (firstChar == ByteConstants.Dash)
            {
                var line = await ReadLine();
                var redisResponse = line.AsString();
                PipeReader.AdvanceTo(line.End);
                throw new RedisFailedCommandException(redisResponse, RedisClient.LastCommand);
            }

            object result;

            if (firstChar == ByteConstants.Asterix)
            {
                var processor = RedisClient.ArrayAbstractAbstractResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Plus)
            {
                var processor = RedisClient.WordAbstractAbstractResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Colon)
            {
                var processor = RedisClient.IntegerAbstractAbstractResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Dollar)
            {
                var processor = RedisClient.DataAbstractAbstractResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else
            {
                var processor = RedisClient.EmptyAbstractAbstractResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }

            return new RawResult(result);
        }
    }
}