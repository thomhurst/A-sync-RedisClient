using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private ConcurrentQueue<Tuple<string, string>> _stringSetQueue = new ConcurrentQueue<Tuple<string, string>>();
        private ConcurrentQueue<Tuple<string, string, int>> _stringSetWithTtlQueue = new ConcurrentQueue<Tuple<string, string, int>>();

        private async Task CollateMultipleRequestsForFireAndForget<T>(T tuple, ConcurrentQueue<T> queue)
        {
            queue.Enqueue(tuple);
            
            // Delay by 1ms to allow other simultaneous calls to collate also
            await Task.Delay(1);
        }
    }
}