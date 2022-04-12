using TomLonghurst.AsyncRedisClient.Models.Backlog;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient
    {
        private readonly BlockingQueue<IBacklog> _backlog = new();

        private void StartBacklogProcessor()
        {
            Task.Run(ProcessBacklog);
        }

        private async Task ProcessBacklog()
        {
            while (true)
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    if (!IsConnected)
                    {
                        await TryConnectAsync(CancellationToken.None).ConfigureAwait(false);
                    }

                    var backlogItems = _backlog.DequeueAll();

                    // Items cancelled will be taken care of by the CancellationToken.Register in the SendOrQueue method
                    var validItems = backlogItems.Where(item => !item.CancellationToken.IsCancellationRequested)
                        .ToList();

                    var pipelinedCommand = validItems
                        .Select(backlogItem => backlogItem.RedisCommand).ToList()
                        .ToPipelinedCommand();

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
                }
                catch
                {
                    DisposeNetwork();
                }
            }
        }
    }
}