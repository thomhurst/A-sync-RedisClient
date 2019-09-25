using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class PipeExtensions
    {
        public static ValueTask<ReadResult> ReadAsyncOrThrowReadTimeout(this PipeReader pipeReader, CancellationToken cancellationToken)
        {
            try
            {
                return pipeReader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                throw new RedisReadTimeoutException(e);
            }
        }
    }
}