using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models.Backlog;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        internal readonly BlockingQueue<IBacklog> _backlog = new BlockingQueue<IBacklog>();

        protected void StartBacklogProcessor()
        {
//            BacklogWorkerThread = new Thread(state => ((RedisClient) state).ProcessBacklog())
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
                var backlogItems = _backlog.DequeueAll();

                var itemsPastTimeout = backlogItems.Where(item => item.CancellationToken.IsCancellationRequested);
                
                foreach (var timedOutItem in itemsPastTimeout)
                {
                    timedOutItem.SetCancelled();
                }
                
                var validItems = backlogItems.Where(item => !itemsPastTimeout.Contains(item)).ToList();
                
                var pipelinedCommand = validItems
                    .Select(backlogItem => backlogItem.RedisCommand).ToList()
                    .ToPipelinedCommand();

                await _sendAndReceiveSemaphoreSlim.WaitAsync();
                
                if (!IsConnected)
                {
                    TryConnectAsync(CancellationToken.None).ConfigureAwait(false);
                }

                try
                {
                    await Write(pipelinedCommand);

                    foreach (var backlogItem in validItems)
                    {
                        await backlogItem.SetResult();
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
                finally
                {
                    _sendAndReceiveSemaphoreSlim.Release();
                }
            }
        }
    }
}