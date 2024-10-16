using System.IO.Pipelines;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

namespace TomLonghurst.AsyncRedisClient.Models.Backlog;

public struct BacklogItem<T> : IBacklogItem<T>
{
    public Client.RedisClient RedisClient { get; set; }
    public PipeReader PipeReader { get; set; }
    public IRedisCommand RedisCommand { get; }
    public CancellationToken CancellationToken { get; }
        
    public void SetCancelled()
    {
        TaskCompletionSource.TrySetCanceled();
    }

    public void SetException(Exception exception)
    {
        TaskCompletionSource.TrySetException(exception);
    }

    public async Task SetResult()
    {
        try
        {
            var result = await AbstractResultProcessor.Start(RedisClient, PipeReader, CancellationToken);
            TaskCompletionSource.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            TaskCompletionSource.TrySetCanceled();
            throw;
        }
        catch (Exception e)
        {
            TaskCompletionSource.TrySetException(e);
            throw;
        }
    }

    public TaskCompletionSource<T> TaskCompletionSource { get; }
    public AbstractResultProcessor<T> AbstractResultProcessor { get; }

    public BacklogItem(IRedisCommand redisCommand, CancellationToken cancellationToken, TaskCompletionSource<T> taskCompletionSource, AbstractResultProcessor<T> abstractResultProcessor, Client.RedisClient redisClient, PipeReader pipe)
    {
        RedisCommand = redisCommand;
        CancellationToken = cancellationToken;
        TaskCompletionSource = taskCompletionSource;
        AbstractResultProcessor = abstractResultProcessor;
        RedisClient = redisClient;
        PipeReader = pipe;
    }
}