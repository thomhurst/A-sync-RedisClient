using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Models.Backlog
{
    public class BacklogItem<T> : IBacklogItem<T>
    {
        public Client.RedisClient RedisClient { get; set; }
        public IDuplexPipe Pipe { get; set; }
        public IRedisCommand RedisCommand { get; }
        public CancellationToken CancellationToken { get; }
        public async Task WriteAndSetResult()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                TaskCompletionSource.TrySetCanceled();
                return;
            }

            try
            {
                RedisClient.LastUsed = DateTime.Now;

                var result =
                    await RedisClient.SendAndReceiveAsync(RedisCommand, ResultProcessor, CancellationToken, false);
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

        public async Task SetResult()
        {
            try
            {
                var result = await ResultProcessor.Start(RedisClient, Pipe);
                TaskCompletionSource.TrySetResult(result);
            }
            catch (Exception e)
            {
                TaskCompletionSource.TrySetException(e);
            }
        }

        public TaskCompletionSource<T> TaskCompletionSource { get; }
        public IResultProcessor<T> ResultProcessor { get; }

        public BacklogItem(IRedisCommand redisCommand, CancellationToken cancellationToken, TaskCompletionSource<T> taskCompletionSource, IResultProcessor<T> resultProcessor, Client.RedisClient redisClient, IDuplexPipe pipe)
        {
            RedisCommand = redisCommand;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
            ResultProcessor = resultProcessor;
            RedisClient = redisClient;
            Pipe = pipe;
        }
    }
}