using System;
using System.Threading;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class CancellationTokenHelper
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CancellationTokenSource CancellationTokenWithTimeout(int timeout, CancellationToken tokenToCombine)
        {
            return CancellationTokenWithTimeout(TimeSpan.FromMilliseconds(timeout), tokenToCombine);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CancellationTokenSource CancellationTokenWithTimeout(TimeSpan timeout, CancellationToken tokenToCombine)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenToCombine);
            //cancellationTokenSource.CancelAfter(timeout);
            return cancellationTokenSource;
        }
    }
}
