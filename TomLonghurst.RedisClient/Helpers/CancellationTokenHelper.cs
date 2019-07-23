using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class CancellationTokenHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CancellationTokenSource CancellationTokenWithTimeout(TimeSpan timeout, CancellationToken tokenToCombine)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenToCombine);
#if !DEBUG
            cancellationTokenSource.CancelAfter(timeout);
#endif
            return cancellationTokenSource;
        }
    }
}
