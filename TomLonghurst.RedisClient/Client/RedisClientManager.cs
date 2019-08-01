using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientManager
    {
        public RedisClientConfig ClientConfig { get; }
        private readonly List<Task<RedisClient>> _redisClients = new List<Task<RedisClient>>();

        public RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
        {
            if (redisClientPoolSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
            }

            ClientConfig = clientConfig;

            for (var i = 0; i < redisClientPoolSize; i++)
            {
                _redisClients.Add(RedisClient.ConnectAsync(clientConfig));
            }
        }

        public async Task<RedisClient> GetRedisClientAsync()
        {
            if (_redisClients.Count == 1)
            {
                var client = await _redisClients.First();

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

            if (_redisClients.Any(task => !task.IsCompleted))
            {
                var leastLoadedAvailableConnection = _redisClients
                    .Where(task => task.IsCompletedSuccessfully())
                    .Select(task => task.Result)
                    .OrderBy(client => client.OutstandingOperations)
                    .FirstOrDefault();

                if (leastLoadedAvailableConnection != null)
                {
                    return leastLoadedAvailableConnection;
                }
                    
                return await await Task.WhenAny(_redisClients);
            }

            var clients = await Task.WhenAll(_redisClients);

            var connectedClientWithLeastOutstandingOperations = clients.OrderBy(client => client.OutstandingOperations).FirstOrDefault(client => client.IsConnected);

            if(connectedClientWithLeastOutstandingOperations != null)
            {
                return connectedClientWithLeastOutstandingOperations;
            }

            return clients.OrderBy(client => client.OutstandingOperations).First();
        }

        public async Task<IEnumerable<RedisClient>> GetAllRedisClientsAsync()
        {
            return await Task.WhenAll(_redisClients);
        }

        public Action<RedisClient> OnConnectionEstablished { get; set; }

        public Action<RedisClient> OnConnectionFailed { get; set; }
    }
}
