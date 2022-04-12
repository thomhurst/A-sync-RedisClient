using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

namespace TomLonghurst.AsyncRedisClient.Models.Backlog
{
    public interface IBacklog
    {
        ReadOnlyMemory<byte> RedisCommand { get; }
        CancellationToken CancellationToken { get; }
        void SetCancelled();
        void SetException(Exception exception);
        Task SetResult();
    }

    public interface IBacklogItem<T> : IBacklog
    {
        TaskCompletionSource<T> TaskCompletionSource { get; }
        
        AbstractResultProcessor<T> AbstractResultProcessor { get; }
    }
}