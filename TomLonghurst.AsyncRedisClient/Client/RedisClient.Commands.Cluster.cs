using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private ClusterCommands _clusterCommands;
        public ClusterCommands Cluster => _clusterCommands;
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
                    var command = RedisEncoder.EncodeCommand(Commands.Cluster, Commands.Info);
                    return await _redisClient.SendOrQueueAsync(command, _redisClient.DataResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}