using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Models.Backlog
{
    public class BacklogItem<T> : IBacklogItem<T>
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
                var result = await ResultProcessor.Start(RedisClient, PipeReader);
                TaskCompletionSource.TrySetResult(result);
            }
            catch (OperationCanceledException)
            {
                TaskCompletionSource.TrySetCanceled();
            }
            catch (Exception e)
            {
                TaskCompletionSource.TrySetException(e);
            }
        }

        public TaskCompletionSource<T> TaskCompletionSource { get; }
        public ResultProcessor<T> ResultProcessor { get; }

        public BacklogItem(IRedisCommand redisCommand, CancellationToken cancellationToken, TaskCompletionSource<T> taskCompletionSource, ResultProcessor<T> resultProcessor, Client.RedisClient redisClient, PipeReader pipe)
        {
            RedisCommand = redisCommand;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
            ResultProcessor = resultProcessor;
            RedisClient = redisClient;
            PipeReader = pipe;
        }
    }
}