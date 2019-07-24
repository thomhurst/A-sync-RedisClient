using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientManager
    {
        public RedisClientConfig ClientConfig { get; }
        private readonly List<Lazy<Task<RedisClient>>> _lazyRedisClients = new List<Lazy<Task<RedisClient>>>();

        public RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
        {
            if (redisClientPoolSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
            }

            ClientConfig = clientConfig;

            for (var i = 0; i < redisClientPoolSize; i++)
            {
                _lazyRedisClients.Add(new Lazy<Task<RedisClient>>(() => RedisClient.ConnectAsync(clientConfig)));
            }
        }

        public async Task<RedisClient> GetRedisClientAsync()
        {
            if (_lazyRedisClients.Count == 1)
            {
                var client = await _lazyRedisClients.First().Value;

                if (client.OnConnectionFailed == null)
                {
                    client.OnConnectionFailed = OnConnectionFailed;
                }

                if (client.OnConnectionEstablished == null)
                {
                    client.OnConnectionEstablished = OnConnectionEstablished;
                }

                return client;
            }
            
            var lazyClientNotYetLoaded = _lazyRedisClients.FirstOrDefault(lazy => !lazy.IsValueCreated);

            if(lazyClientNotYetLoaded != null)
            {
                var redisClient = await lazyClientNotYetLoaded.Value;
                
                redisClient.OnConnectionFailed = OnConnectionFailed;
                redisClient.OnConnectionEstablished = OnConnectionEstablished;
                
                return redisClient;
            }

            var clientTasks = _lazyRedisClients.Select(lazyClient => lazyClient.Value).ToList();

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
            return await Task.WhenAll(_lazyRedisClients.Select(lazyRedisClient => lazyRedisClient.Value));
        }

        public Action<RedisClient> OnConnectionEstablished { get; set; }

        public Action<RedisClient> OnConnectionFailed { get; set; }
    }
}
