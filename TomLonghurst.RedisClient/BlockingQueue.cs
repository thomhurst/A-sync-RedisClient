using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient
{
    public class BlockingQueue<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> _innerQueue;

        public int Count => _innerQueue.Count;

        private int _availableToDequeue;

        public BlockingQueue()
        {
            _innerQueue = new ConcurrentQueue<T>();
        }

        public void Enqueue(T item)
        {
            _innerQueue.Enqueue(item);
            
            if (_availableToDequeue != 0)
            {
                Monitor.Pulse(this);
            }
        }

        public void EnqueueRange(IEnumerable<T> source)
        {
            var n = 0;
            foreach (var item in source)
            {
                _innerQueue.Enqueue(item);
                
                if (_availableToDequeue != 0)
                {
                    Monitor.Pulse(this);
                }
                
                n++;
            }
        }

        public T Dequeue()
        {
            // Used to avoid returning null
            while (true)
            {
                while (Count == 0)
                {
                    _availableToDequeue++;
                    Monitor.Wait(this);
                    _availableToDequeue--;
                }

                if (_innerQueue.TryDequeue(out var item))
                {
                    return item;
                }
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                Monitor.PulseAll(this);
            }
        }
    }
}