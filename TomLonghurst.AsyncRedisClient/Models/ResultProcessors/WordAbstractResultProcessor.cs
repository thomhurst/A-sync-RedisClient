using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class WordResultProcessor : AbstractResultProcessor<string>
{
    internal override async ValueTask<string> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var line = await ReadLine(pipeReader, cancellationToken);

        if (line.ItemAt(0) != ByteConstants.Plus)
        {
            var stringLine = line.AsStringWithoutLineTerminators();
                
            pipeReader.AdvanceTo(line.End);
                
            if (line.ItemAt(0) == ByteConstants.Dash)
            {
                throw new RedisFailedCommandException(stringLine, redisClient.LastCommand);
            }
                
            throw new UnexpectedRedisResponseException(stringLine);
        }

        var word = line.Slice(line.GetPosition(1, line.Start)).AsStringWithoutLineTerminators();
        
        pipeReader.AdvanceTo(line.End);
        
        return word;
    }
}