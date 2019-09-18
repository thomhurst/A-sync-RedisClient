using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Models.Backlog
{
    public interface IBacklog
    {
        IRedisCommand RedisCommand { get; }
        CancellationToken CancellationToken { get; }
        void SetCancelled();
        void SetException(Exception exception);
        Task SetResult();
    }

    public interface IBacklogItem<T> : IBacklog
    {
        TaskCompletionSource<T> TaskCompletionSource { get; }
        
        ResultProcessor<T> ResultProcessor { get; }
    }
}