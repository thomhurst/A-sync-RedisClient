using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Models.Backlog
{
    public class BacklogItem : IBacklogItem
    {
        public Client.RedisClient RedisClient { get; }
        public IDuplexPipe Pipe { get; }
        public IRedisCommand RedisCommand { get; }
        public CancellationToken CancellationToken { get; }

        public Task WriteAndSetResult()
        {
            throw new NotImplementedException();
        }

        public void SetClientAndPipe(Client.RedisClient redisClient, IDuplexPipe pipe)
        {
            throw new NotImplementedException();
        }

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
                await RedisClient.Write(RedisCommand);
                
                if (!Pipe.Input.TryRead(out var readResult))
                {
                    readResult = await Pipe.Input.ReadAsync().ConfigureAwait(false);
                }

                var result = await ResultProcessor.Start(RedisClient, Pipe, readResult);
            
                TaskCompletionSource.TrySetResult(result);
            }
            catch (Exception e)
            {
                TaskCompletionSource.TrySetException(e);
            }
        }

        public void SetClientAndPipe(Client.RedisClient redisClient, IDuplexPipe pipe)
        {
            RedisClient = redisClient;
            Pipe = pipe;
        }

        public TaskCompletionSource<T> TaskCompletionSource { get; }
        public IResultProcessor<T> ResultProcessor { get; }

        public BacklogItem(IRedisCommand redisCommand, CancellationToken cancellationToken, TaskCompletionSource<T> taskCompletionSource, IResultProcessor<T> resultProcessor)
        {
            RedisCommand = redisCommand;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
            ResultProcessor = resultProcessor;
        }
    }
}