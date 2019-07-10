using System.Threading;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class ApplicationStats
    {
        internal static void GetThreadPoolStats(out string ioThreadStats, out string workerThreadStats)
        {
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxIoThreads);
            ThreadPool.GetAvailableThreads(out var freeWorkerThreads, out var freeIoThreads);
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minIoThreads);

            var busyIoThreads = maxIoThreads - freeIoThreads;
            var busyWorkerThreads = maxWorkerThreads - freeWorkerThreads;

            ioThreadStats = $"IO Threads: Busy={busyIoThreads}|Free={freeIoThreads}|Min={minIoThreads}|Max={maxIoThreads}";
            workerThreadStats = $"Worker Threads: Busy={busyWorkerThreads}|Free={freeWorkerThreads}|Min={minWorkerThreads}|Max={maxWorkerThreads}";
        }
    }
}