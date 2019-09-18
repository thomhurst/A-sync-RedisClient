using System;
using System.Collections.Concurrent;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private ConcurrentQueue<Tuple<string, string>> _stringSetQueue = new ConcurrentQueue<Tuple<string, string>>();
        private ConcurrentQueue<Tuple<string, string, int>> _stringSetWithTtlQueue = new ConcurrentQueue<Tuple<string, string, int>>();

        private void CollateMultipleRequestsForPipelining<T>(T tuple, ConcurrentQueue<T> queue)
        {
            queue.Enqueue(tuple);
        }
    }
}