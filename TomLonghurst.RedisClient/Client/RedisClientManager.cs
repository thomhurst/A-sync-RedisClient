using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientManager
    {
        private readonly List<AsyncLazy<RedisClient>> _lazyRedisClients = new List<AsyncLazy<RedisClient>>();

        public RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
        {
            if (redisClientPoolSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
            }

            for (var i = 0; i < redisClientPoolSize; i++)
            {
                _lazyRedisClients.Add(new AsyncLazy<RedisClient>(() => RedisClient.ConnectAsync(clientConfig)));
            }
        }

        public async Task<RedisClient> GetRedisClientAsync()
        {
            if (_lazyRedisClients.Count == 1)
            {
                return await _lazyRedisClients.First().Task;
            }
            
            var lazyClientNotYetLoaded = _lazyRedisClients.FirstOrDefault(lazy => !lazy.IsStarted);

            if(lazyClientNotYetLoaded != null)
            {
                return await lazyClientNotYetLoaded.Task;
            }

            var clientTasks = _lazyRedisClients.Select(lazyClient => lazyClient.Task).ToList();

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
            return await Task.WhenAll(_lazyRedisClients.Select(lazyRedisClient => lazyRedisClient.Task));
        }
    }
}
