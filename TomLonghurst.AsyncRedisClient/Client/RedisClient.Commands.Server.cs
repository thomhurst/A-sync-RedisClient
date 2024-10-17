using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Client;

public partial class RedisClient : IDisposable
{
    public ServerCommands Server { get; }

    public class ServerCommands
    {
        private readonly RedisClient _redisClient;

        internal ServerCommands(RedisClient redisClient)
        {
            _redisClient = redisClient;
        }
            
        public ValueTask<string> Info()
        {
            return Info(CancellationToken.None);
        }
        
        public async ValueTask<string> Info(CancellationToken cancellationToken)
        {
            return await  _redisClient.RunWithTimeout(async token =>
            {
                return await _redisClient.SendOrQueueAsync(Commands.Info, _redisClient.DataResultProcessor, CancellationToken.None);
            }, cancellationToken);
        }

        public Task<int> DbSize()
        {
            return DbSize(CancellationToken.None);
        }

        public async Task<int> DbSize(CancellationToken cancellationToken)
        {
            return await _redisClient.RunWithTimeout(async token =>
            {
                return await _redisClient.SendOrQueueAsync(Commands.DbSize, _redisClient.IntegerResultProcessor, CancellationToken.None);
            }, cancellationToken);
        }
    }
}