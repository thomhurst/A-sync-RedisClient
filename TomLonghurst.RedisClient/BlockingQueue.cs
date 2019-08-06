using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient
{
    public class BlockingQueue<T> : IDisposable
    {
        private readonly object _locker = new object();
        private readonly ConcurrentQueue<T> _innerQueue;

        public int Count => _innerQueue.Count;

        private int _availableToDequeue;
        private bool _disposed;

        public BlockingQueue()
        {
            _innerQueue = new ConcurrentQueue<T>();
        }

        public void Enqueue(T item)
        {
            _innerQueue.Enqueue(item);

            lock (_locker)
            {
                if (_availableToDequeue != 0)
                {
                    Monitor.Pulse(_locker);
                }
            }
        }

        public void EnqueueRange(IEnumerable<T> source)
        {
            var n = 0;
            foreach (var item in source)
            {
                lock (_locker)
                {
                    _innerQueue.Enqueue(item);

                    if (_availableToDequeue != 0)
                    {
                        Monitor.Pulse(_locker);
                    }
                }

                n++;
            }
        }

        public T Dequeue()
        {
            // Used to avoid returning null
            while (true)
            {
                lock (_locker)
                {
                    while (Count == 0)
                    {
                        if (_disposed)
                        {
                            return default;
                        }
                        
                        _availableToDequeue++;
                        Monitor.Wait(_locker);
                        _availableToDequeue--;
                    }

                    if (_innerQueue.TryDequeue(out var item))
                    {
                        return item;
                    }
                }
            }
        }
        
        public List<T> DequeueAll()
        {
            // Used to avoid returning null
            while (true)
            {
                lock (_locker)
                {
                    while (Count == 0)
                    {
                        if (_disposed)
                        {
                            return default;
                        }
                        
                        _availableToDequeue++;
                        Monitor.Wait(_locker);
                        _availableToDequeue--;
                    }
                    
                    var list = new List<T>();
                    
                    var hasItem = true;
                    while (hasItem)
                    {
                        hasItem = _innerQueue.TryDequeue(out var item);
                        if (hasItem)
                        {
                            list.Add(item);
                        }
                        else
                        {
                            return list;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
            
            lock (_locker)
            {
                Monitor.PulseAll(_locker);
            }
        }
    }
}