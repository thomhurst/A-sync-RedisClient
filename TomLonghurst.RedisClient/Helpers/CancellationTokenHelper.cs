using System;
using System.Threading;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class CancellationTokenHelper
    {
        internal static CancellationToken CancellationTokenWithTimeout(int timeout, CancellationToken tokenToCombine)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenToCombine);
            cancellationTokenSource.CancelAfter(timeout);
            return cancellationTokenSource.Token;
        }

        internal static CancellationToken CancellationTokenWithTimeout(TimeSpan timeout, CancellationToken tokenToCombine)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenToCombine);
            cancellationTokenSource.CancelAfter(timeout);
            return cancellationTokenSource.Token;
        }
    }
}
