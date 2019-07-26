using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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

        private List<Thread> _threads = new List<Thread>(); 
        
        /// <summary>
        /// Create a new dedicated thread-pool
        /// </summary>
        public PipeThreadPoolScheduler(string name = null, int workerCount = 5)
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

        private readonly ConcurrentQueue<WorkItem> _queue = new ConcurrentQueue<WorkItem>();
        private void StartWorker(int id)
        {
            var thread = new Thread(ThreadRunWorkLoop)
            {
                Name = $"{nameof(PipeThreadPoolScheduler)}:{id}",
                Priority = ThreadPriority.Normal,
                IsBackground = true
            };
            
            _threads.Add(thread);
            
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

            if (!_disposed && _queue.Count <= WorkerCount)
            {
                _queue.Enqueue(new WorkItem(action, state));
            }
            else
            {
                ThreadPool.Schedule(action, state);
            }
        }

        private static readonly ParameterizedThreadStart ThreadRunWorkLoop = state => ((PipeThreadPoolScheduler)state).RunWorkLoop();

        private int _availableCount;
        /// <summary>
        /// The number of workers currently actively engaged in work
        /// </summary>
        public int AvailableCount => Thread.VolatileRead(ref _availableCount);

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
                if (_queue.TryDequeue(out var next))
                {
                    Execute(next.Action, next.State);
                }

                // Finish Queue Before we kill the thread
                if (_queue.Count == 0 && _disposed)
                {
                    return;
                }
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
        }
    }
}
