using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Models.Backlog
{
    public class BacklogItem : IBacklogItem
    {
        public IRedisCommand RedisCommand { get; }
        public CancellationToken CancellationToken { get; }
        public TaskCompletionSource<bool> TaskCompletionSource { get; }

        public BacklogItem(IRedisCommand redisCommand, CancellationToken cancellationToken, TaskCompletionSource<bool> taskCompletionSource)
        {
            RedisCommand = redisCommand;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
        }
    }

    public class BacklogItem<T> : IBacklogItem<T>
    {
        public IRedisCommand RedisCommand { get; }
        public CancellationToken CancellationToken { get; }
        
        public TaskCompletionSource<T> TaskCompletionSource { get; }
     
        public BacklogItem(IRedisCommand redisCommand, CancellationToken cancellationToken, TaskCompletionSource<T> taskCompletionSource)
        {
            RedisCommand = redisCommand;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
        }
    }
}