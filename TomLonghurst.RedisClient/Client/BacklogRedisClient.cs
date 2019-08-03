using System.Threading.Tasks;
using TomLonghurst.RedisClient.Enums;

namespace TomLonghurst.RedisClient.Client
{
    internal class BacklogRedisClient : RedisClient
    {
        protected BacklogRedisClient(RedisClientConfig redisClientConfig) : base(redisClientConfig)
        {
        }

        protected BacklogRedisClient(RedisClientConfig redisClientConfig, ClientType clientType) : base(redisClientConfig, clientType)
        {
        }

        protected BacklogRedisClient(ClientType clientType) : base(clientType)
        {
        }

        protected override async Task StartBacklogProcessor()
        {
            await ProcessBacklog();
        }

        internal override bool CanQueueToBacklog { get; set; } = false;
    }
}