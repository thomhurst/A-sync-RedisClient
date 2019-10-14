using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class PipeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ValueTask<ReadResult> ReadAsyncOrThrowReadTimeout(this PipeReader pipeReader, CancellationToken cancellationToken)
        {
            try
            {
                if (pipeReader.TryRead(out var readResult))
                {
                    return readResult;
                }
                
                return await pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                throw new RedisReadTimeoutException(e);
            }
        }
    }
}