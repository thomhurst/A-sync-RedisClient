using System.Collections.Immutable;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public class RedisClientManager
    {
        public RedisClientConfig ClientConfig { get; }
        private readonly List<RedisClient> _redisClients;

        public RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
        {
            if (redisClientPoolSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
            }

            _redisClients = new List<RedisClient>(redisClientPoolSize);

            ClientConfig = clientConfig;

            for (var i = 0; i < redisClientPoolSize; i++)
            {
                _redisClients.Add(RedisClient.Create(clientConfig));
            }
        }

        public RedisClient GetRedisClient()
        {
            if (_redisClients.Count == 1)
            {
                return _redisClients.First();
            }

            return _redisClients.Where(x => x.IsConnected)
                       .OrderBy(x => x.OutstandingOperations)
                       .FirstOrDefault()
                   ?? _redisClients.First();
        }

        private void SetConnectionCallbacks()
        {
            foreach (var client in _redisClients)
            {
                if (client.OnConnectionFailed == null)
                {
                    client.OnConnectionFailed = OnConnectionFailed;
                }

                if (client.OnConnectionEstablished == null)
                {
                    client.OnConnectionEstablished = OnConnectionEstablished;
                }
            }
        }

        public ImmutableArray<RedisClient> GetAllRedisClients()
        {
            return _redisClients.ToImmutableArray();
        }

        private Func<RedisClient, Task> _onConnectionEstablished;
        public Func<RedisClient, Task> OnConnectionEstablished
        {
            get => _onConnectionEstablished;
            set
            {
                _onConnectionEstablished = value;
                SetConnectionCallbacks();
            }
        }

        private Func<RedisClient, Task> _onConnectionFailed;
        public Func<RedisClient, Task> OnConnectionFailed
        {
            get => _onConnectionFailed;
            set
            {
                _onConnectionFailed = value;
                SetConnectionCallbacks();
            }
        }
    }
}
