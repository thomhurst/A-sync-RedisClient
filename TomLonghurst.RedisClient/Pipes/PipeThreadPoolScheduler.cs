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
        }

        private long _totalServicedByQueue, _totalServicedByPool;



        /// <summary>
        /// Requests <paramref name="action"/> to be run on scheduler with <paramref name="state"/> being passed in
        /// </summary>
        public override void Schedule(Action<object> action, object state)
        {
            if (action == null)
            {
                return; // nothing to do
            }

            Task.Factory.StartNew(action, state);
        }

        private int _availableCount;
        /// <summary>
        /// The number of workers currently actively engaged in work
        /// </summary>


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
            
        }
    }
}
