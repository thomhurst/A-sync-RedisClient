using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Models.Backlog
{
    public interface IBacklog
    {
        IRedisCommand RedisCommand { get; }
        CancellationToken CancellationToken { get; }
        void SetCancelled();
        Task SetResult();
    }

    public interface IBacklogItem<T> : IBacklog
    {
        TaskCompletionSource<T> TaskCompletionSource { get; }
        
        ResultProcessor<T> ResultProcessor { get; }
    }
}