using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

public class ArrayResultProcessor : AbstractResultProcessor<IEnumerable<StringRedisValue>>
{
    internal override async ValueTask<IEnumerable<StringRedisValue>> Process(
        RedisClient redisClient, 
        PipeReader pipeReader, 
        ReadResult readResult,
        CancellationToken cancellationToken
    )
    {
        var line = await ReadLine(pipeReader, cancellationToken);

        if (line.ItemAt(0) != ByteConstants.Asterix)
        {
            var stringLine = line.AsStringWithoutLineTerminators();
            pipeReader.AdvanceTo(line.End);
            throw new UnexpectedRedisResponseException(stringLine);
        }

        var count = SpanNumberParser.Parse(line);

        pipeReader.AdvanceTo(line.End);

        var results = new byte [count][];
        
        for (var i = 0; i < count; i++)
        {
            results[i] = (await ReadData(redisClient, pipeReader, readResult, cancellationToken)).ToArray();
        }

        return results.ToRedisValues();
    }
}