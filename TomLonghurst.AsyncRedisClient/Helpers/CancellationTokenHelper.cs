namespace TomLonghurst.AsyncRedisClient.Helpers;

public static class CancellationTokenHelper
{
        
    internal static CancellationTokenSource CancellationTokenWithTimeout(TimeSpan timeout, CancellationToken tokenToCombine)
    {
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenToCombine);
#if !DEBUG
            cancellationTokenSource.CancelAfter(timeout);
#endif
        return cancellationTokenSource;
    }
}