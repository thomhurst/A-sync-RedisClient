using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Client
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
                    return await _redisClient.SendAndReceiveAsync(Commands.ClusterInfo, _redisClient.DataResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}