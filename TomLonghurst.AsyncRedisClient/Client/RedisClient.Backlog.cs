using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Models.Backlog;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient
    {
        private readonly BlockingQueue<IBacklog> _backlog = new BlockingQueue<IBacklog>();

        protected void StartBacklogProcessor()
        {
//            BacklogWorkerThread = new Thread(async state => await ((RedisClient) state).ProcessBacklog())
//            {
//                Name = $"{nameof(RedisClient)}",
//                Priority = ThreadPriority.Normal,
//                IsBackground = true
//            };
//                
//            BacklogWorkerThread.Start(this);
        }

        internal async Task ProcessBacklog()
        {
            while (true)
            {
                if (_disposed)
                {
                    return;
                }
                
                var backlogItems = _backlog.DequeueAll();

                var itemsPastTimeout = backlogItems.Where(item => item.CancellationToken.IsCancellationRequested).ToList();
                
                foreach (var timedOutItem in itemsPastTimeout)
                {
                    timedOutItem.SetCancelled();
                }
                
                var validItems = backlogItems.Where(item => !itemsPastTimeout.Contains(item)).ToList();
                
                var pipelinedCommand = validItems
                    .Select(backlogItem => backlogItem.RedisCommand).ToList()
                    .ToPipelinedCommand();

                if (!IsConnected)
                {
                    await TryConnectAsync(CancellationToken.None).ConfigureAwait(false);
                }

                try
                {
                    await Write(pipelinedCommand);

                    foreach (var backlogItem in validItems)
                    {
                        try
                        {
                            await backlogItem.SetResult();
                        }
                        catch (Exception e)
                        {
                            backlogItem.SetException(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    foreach (var validItem in validItems)
                    {
                        validItem.SetException(e);
                    }

                    DisposeNetwork();
                }
            }
        }
    }
}