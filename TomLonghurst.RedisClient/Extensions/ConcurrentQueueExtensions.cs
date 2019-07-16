using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class ConcurrentQueueExtensions
    {
        public static IList<T> DequeueAll<T>(this ConcurrentQueue<T> queue)
        {
            lock (queue)
            {
                var list = new List<T>();

                while (queue.TryDequeue(out var result) && list.Count < 500)
                {
                    list.Add(result);
                }

                return list;
            }
        }
    }
}