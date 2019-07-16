using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class ConcurrentQueueExtensions
    {
        public static async Task<IList<T>> DequeueAll<T>(this ConcurrentQueue<T> queue, SemaphoreSlim sendSemaphoreSlim,
            CancellationToken cancellationToken)
        {
            await sendSemaphoreSlim.WaitAsync(cancellationToken);

            var list = new List<T>();

            while (queue.TryDequeue(out var result) && list.Count < 500)
            {
                list.Add(result);
            }

            sendSemaphoreSlim.Release();

            return list;
        }
    }
}