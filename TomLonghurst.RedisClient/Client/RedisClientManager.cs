using System;
using System.Collections.Generic;
using System.Linq;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientManager
    {
        public RedisClientConfig ClientConfig { get; }
        private readonly List<RedisClient> _redisClients = new List<RedisClient>();

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

        public RedisClient GetRedisClient()
        {
            if (_redisClients.Count == 1)
            {
                var client = _redisClients.First();

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

            var connectedClientWithLeastOutstandingOperations = _redisClients.OrderBy(client => client.OutstandingOperations).FirstOrDefault(client => client.IsConnected);

            if(connectedClientWithLeastOutstandingOperations != null)
            {
                return connectedClientWithLeastOutstandingOperations;
            }

            return _redisClients.OrderBy(client => client.OutstandingOperations).First();
        }

        public IEnumerable<RedisClient> GetAllRedisClients()
        {
            return _redisClients;
        }

        public Action<RedisClient> OnConnectionEstablished { get; set; }

        public Action<RedisClient> OnConnectionFailed { get; set; }
    }
}
