using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient
{
    public class AsyncQueue<T> : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ConcurrentQueue<T> _innerQueue;

        public int Count => _innerQueue.Count;

        public AsyncQueue()
        {
            _semaphoreSlim = new SemaphoreSlim(0);
            _innerQueue = new ConcurrentQueue<T>();
        }

        public void Enqueue(T item)
        {
            _innerQueue.Enqueue(item);
            _semaphoreSlim.Release();
        }

        public void EnqueueRange(IEnumerable<T> source)
        {
            var n = 0;
            foreach (var item in source)
            {
                _innerQueue.Enqueue(item);
                n++;
            }

            _semaphoreSlim.Release(n);
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Used to avoid returning null
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
             
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                if (_innerQueue.TryDequeue(out var item))
                {
                    return item;
                }
            }
        }

        ~AsyncQueue()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            _semaphoreSlim?.Dispose();
        }
    }
}