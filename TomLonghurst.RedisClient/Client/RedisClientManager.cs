using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientManager
    {
        private readonly List<Task<RedisClient>> _lazyRedisClients = new List<Task<RedisClient>>();

        public RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
        {
            if (redisClientPoolSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
            }

            for (var i = 0; i < redisClientPoolSize; i++)
            {
                _lazyRedisClients.Add(RedisClient.ConnectAsync(clientConfig));
            }
        }

        public async Task<RedisClient> GetRedisClientAsync()
        {
//            if (_lazyRedisClients.Count == 1)
//            {
//                return await _lazyRedisClients.First().Value;
//            }
//            
//            var lazyClientNotYetLoaded = _lazyRedisClients.FirstOrDefault(lazy => !lazy.IsValueCreated);
//
//            if(lazyClientNotYetLoaded != null)
//            {
//                return await lazyClientNotYetLoaded.Value;
//            }
//
//            var clientTasks = _lazyRedisClients.Select(lazyClient => lazyClient.Value).ToList();
            
            var clientTasks = _lazyRedisClients;

            if (clientTasks.Any(task => !task.IsCompleted))
            {
                return await await Task.WhenAny(clientTasks);
            }

            var clients = await Task.WhenAll(clientTasks);

            var connectedClientWithLeastOutstandingOperations = clients.OrderBy(client => client.OutstandingOperations).FirstOrDefault(client => client.IsConnected);

            if(connectedClientWithLeastOutstandingOperations != null)
            {
                return connectedClientWithLeastOutstandingOperations;
            }

            return clients.OrderBy(client => client.OutstandingOperations).First();
        }

        public async Task<IEnumerable<RedisClient>> GetAllRedisClientsAsync()
        {
            return await Task.WhenAll(_lazyRedisClients);
        }
    }
}
