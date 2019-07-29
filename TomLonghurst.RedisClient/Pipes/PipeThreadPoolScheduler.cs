using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TomLonghurst.RedisClient.Pipes
{
    /// <summary>
    /// An implementation of a pipe-scheduler that uses a dedicated pool of threads, deferring to
    /// the thread-pool if that becomes too backlogged
    /// </summary>
    public sealed class PipeThreadPoolScheduler : PipeScheduler, IDisposable
    {
        /// <summary>
        /// Reusable shared scheduler instance
        /// </summary>
        public static PipeThreadPoolScheduler Default => StaticContext.Instance;

        private static class StaticContext
        {   // locating here rather than as a static field on DedicatedThreadPoolPipeScheduler so that it isn't instantiated too eagerly
            internal static readonly PipeThreadPoolScheduler Instance = new PipeThreadPoolScheduler(nameof(Default));
        }

        [ThreadStatic]
        private static int s_threadWorkerPoolId;
        private static int s_nextWorkerPoolId;

        /// <summary>
        /// Indicates whether the current thread is a worker, optionally for the specific pool
        /// (otherwise for any pool)
        /// </summary>
        public static bool IsWorker(PipeThreadPoolScheduler pool = null)
            => pool == null ? s_threadWorkerPoolId != 0 : s_threadWorkerPoolId == pool.Id;

        private int Id { get; }

        /// <summary>
        /// The name of the pool
        /// </summary>
        public override string ToString() => Name;

        /// <summary>
        /// The number of workers associated with this pool
        /// </summary>
        public int WorkerCount { get; }

        //private int UseThreadPoolQueueLength { get; }

        private string Name { get; }
        
        /// <summary>
        /// Create a new dedicated thread-pool
        /// </summary>
        public PipeThreadPoolScheduler(string name = null, int workerCount = 2)
        {
            if (workerCount <= 0)
            {
                throw new ArgumentNullException(nameof(workerCount));
            }
            
            Id = Interlocked.Increment(ref s_nextWorkerPoolId);

            WorkerCount = workerCount;
            //UseThreadPoolQueueLength = useThreadPoolQueueLength;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = GetType().Name;
            }
            
            Name = name.Trim();

            for (var i = 0; i < workerCount; i++)
            {
                StartWorker(i);
            }
        }

        private long _totalServicedByQueue, _totalServicedByPool;

        /// <summary>
        /// The total number of operations serviced by the queue
        /// </summary>
        public long TotalServicedByQueue => Volatile.Read(ref _totalServicedByQueue);

        /// <summary>
        /// The total number of operations that could not be serviced by the queue, but which were sent to the thread-pool instead
        /// </summary>
        public long TotalServicedByPool => Volatile.Read(ref _totalServicedByPool);

        private readonly struct WorkItem
        {
            public readonly Action<object> Action;
            public readonly object State;
            public WorkItem(Action<object> action, object state)
            {
                Action = action;
                State = state;
            }
        }

        private volatile bool _disposed;

        private readonly Queue<WorkItem> _queue = new Queue<WorkItem>();
        private void StartWorker(int id)
        {
            var thread = new Thread(ThreadRunWorkLoop)
            {
                Name = $"{nameof(PipeThreadPoolScheduler)}:{id}",
                Priority = ThreadPriority.Normal,
                IsBackground = true
            };
            
            thread.Start(this);
        }

        /// <summary>
        /// Requests <paramref name="action"/> to be run on scheduler with <paramref name="state"/> being passed in
        /// </summary>
        public override void Schedule(Action<object> action, object state)
        {
            if (action == null)
            {
                return; // nothing to do
            }

            lock (_queue)
            {
                if (!_disposed && _queue.Count <= WorkerCount * 2)
                {
                    _queue.Enqueue(new WorkItem(action, state));

                    if (_availableThreads != 0)
                    {
                        // Wake up a thread to do some work
                        Monitor.Pulse(_queue);
                    }
                    
                    return;
                }
            }
            
            // If condition above not met - We'll go to the Global ThreadPool
            ThreadPool.Schedule(action, state);
        }

        private static readonly ParameterizedThreadStart ThreadRunWorkLoop = state => ((PipeThreadPoolScheduler)state).RunWorkLoop();

        private int _availableThreads;
        /// <summary>
        /// The number of workers currently actively engaged in work
        /// </summary>
        public int AvailableCount => Thread.VolatileRead(ref _availableThreads);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Execute(Action<object> action, object state)
        {
            try
            {
                action(state);
            }
            catch (Exception ex)
            {
                
            }
        }

        private void RunWorkLoop()
        {
            s_threadWorkerPoolId = Id;
            while (true)
            {
                WorkItem next;
                lock (_queue)
                {
                    while (_queue.Count == 0)
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        // Thread is paused and available to be woken up to do some work
                        _availableThreads++;
                        Monitor.Wait(_queue);
                        _availableThreads--;
                    }

                    if (_queue.Count == 0)
                    {
                        if (_disposed)
                        {
                            // Only break once the queue has been emptied
                            break;
                        }

                        continue;
                    }

                    next = _queue.Dequeue();
                }

                Execute(next.Action, next.State);
            }
        }

        ~PipeThreadPoolScheduler()
        {
            Dispose();
        }
        
        /// <summary>
        /// Release the threads associated with this pool; if additional work is requested, it will
        /// be sent to the main thread-pool
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            lock (_queue)
            {
                // Resume all available and waiting threads so that they can exit/shut down
                Monitor.PulseAll(_queue);
            }
        }
    }
}
