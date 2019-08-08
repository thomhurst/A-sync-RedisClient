using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Models.Backlog;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        private WeakReference<RedisClient> _weakReference;

        internal readonly BlockingQueue<IBacklog> _backlog = new BlockingQueue<IBacklog>();

        private readonly Action<object> _processBacklogAction = async obj =>
        {
            var wr = (WeakReference<RedisClient>) obj;
            if (wr.TryGetTarget(out var redisClient))
            {
                await redisClient.ProcessBacklog();
            }
        };

        protected void StartBacklogProcessor()
        {
            //_backlogScheduler.Schedule(_processBacklogAction, _weakReference);
        }

        internal async Task ProcessBacklog()
        {
            while (true)
            {
                var backlogItems = _backlog.DequeueAll();
                
                var pipelinedCommand = backlogItems.Select(backlogItem => backlogItem.RedisCommand).ToList()
                    .ToPipelinedCommand();

                await _sendAndReceiveSemaphoreSlim.WaitAsync();
                
                await Write(pipelinedCommand);
                
                foreach (var backlogItem in backlogItems)
                {
                    await backlogItem.SetResult();
                }

                _sendAndReceiveSemaphoreSlim.Release();
            }
        }

        private async Task WriteAndReceiveBacklog(IBacklog backlogItem)
        {
            await backlogItem.WriteAndSetResult();
        }
    }
}