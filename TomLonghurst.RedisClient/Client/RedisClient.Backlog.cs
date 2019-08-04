using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Models.Backlog;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        private WeakReference<RedisClient> _weakReference;

        internal readonly ConcurrentQueue<IBacklog> _backlog = new ConcurrentQueue<IBacklog>();
        
        private static readonly Action<object> _processBacklogAction = s =>
        {
            var wr = (WeakReference<RedisClient>)s;
            if (wr.TryGetTarget(out var redisClient))
            {
                if (!redisClient.IsBacklogProcessorRunning)
                {
                    redisClient.ProcessBacklog();
                }
            }
        };

        protected virtual Task StartBacklogProcessor()
        {
            _pipeScheduler.Schedule(_processBacklogAction, _weakReference);
            return Task.CompletedTask;
        }

        private bool IsBacklogProcessorRunning;
        internal async Task ProcessBacklog()
        {
            if (!IsBacklogProcessorRunning)
            {
                IsBacklogProcessorRunning = true;
                if (_backlog.Count > 0)
                {
                    while (_backlog.Count > 0)
                    {
                        if (_backlog.TryDequeue(out var backlogItem))
                        {
                            await WriteAndReceiveBacklog(backlogItem);
                        }
                    }
                }

                IsBacklogProcessorRunning = false;
            }
        }

        private async Task WriteAndReceiveBacklog(IBacklog backlogItem)
        {
            backlogItem.SetClientAndPipe(this, _pipe);
            await backlogItem.WriteAndSetResult();
        }
    }
}