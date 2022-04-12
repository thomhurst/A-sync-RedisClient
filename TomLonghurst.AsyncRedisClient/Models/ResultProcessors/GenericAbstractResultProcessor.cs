using System.Text;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors
{
    public class GenericResultProcessor : AbstractResultProcessor<RawResult>
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
                throw new RedisFailedCommandException(redisResponse, Encoding.UTF8.GetString(RedisClient.LastCommand.ToArray()));
            }

            object result;

            if (firstChar == ByteConstants.Asterix)
            {
                var processor = RedisClient.ArrayResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Plus)
            {
                var processor = RedisClient.WordResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Colon)
            {
                var processor = RedisClient.IntegerResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else if (firstChar == ByteConstants.Dollar)
            {
                var processor = RedisClient.DataResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }
            else
            {
                var processor = RedisClient.EmptyResultProcessor;
                processor.SetMembers(RedisClient, PipeReader, ReadResult, CancellationToken);
                result = await processor.Process();
            }

            return new RawResult(result);
        }
    }
}