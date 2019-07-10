using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class CancellationTokenHelper
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CancellationToken CancellationTokenWithTimeout(int timeout, CancellationToken tokenToCombine)
        {
            return CancellationTokenWithTimeout(TimeSpan.FromMilliseconds(timeout), tokenToCombine);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CancellationToken CancellationTokenWithTimeout(TimeSpan timeout, CancellationToken tokenToCombine)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenToCombine);
            cancellationTokenSource.CancelAfter(timeout);
            return cancellationTokenSource.Token;
        }
    }
}
