using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                // TODO Refactor .Result
                _redisClients.Add(RedisClient.ConnectAsync(clientConfig));
            }
        }

        public async Task<RedisClient> GetRedisClient()
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

            var redisClientsLoaded = _redisClients.Where(x => x.IsCompleted).ToList();

            if (redisClientsLoaded.Count != _redisClients.Count)
            {
                return await await Task.WhenAny(_redisClients);
            }

            var clients = _redisClients.Select(x => x.Result);
            
            var connectedClientWithLeastOutstandingOperations = clients.OrderBy(client => client.OutstandingOperations).FirstOrDefault(client => client.IsConnected);

            if(connectedClientWithLeastOutstandingOperations != null)
            {
                return connectedClientWithLeastOutstandingOperations;
            }

            return clients.OrderBy(client => client.OutstandingOperations).First();
        }

        public List<Task<RedisClient>> GetAllRedisClients()
        {
            return _redisClients;
        }

        public Action<RedisClient> OnConnectionEstablished { get; set; }

        public Action<RedisClient> OnConnectionFailed { get; set; }
    }
}
