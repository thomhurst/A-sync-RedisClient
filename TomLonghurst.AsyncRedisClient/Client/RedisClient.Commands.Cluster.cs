using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Client;

public partial class RedisClient : IDisposable
{
    public ClusterCommands Cluster { get; }

    public class ClusterCommands
    {
        private readonly RedisClient _redisClient;

        internal ClusterCommands(RedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        public Task<string> ClusterInfoAsync()
        {
            return ClusterInfoAsync(CancellationToken.None);
        }
        
        public async Task<string> ClusterInfoAsync(CancellationToken cancellationToken)
        {
            return await _redisClient.RunWithTimeout(async token =>
            {
                return await _redisClient.SendOrQueueAsync(Commands.ClusterInfo, _redisClient.DataResultProcessor, token);
            }, cancellationToken);
        }
    }
}