using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Client
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
                return await _redisClients.First();
            }

            var redisClientsLoaded = _redisClients.Where(x => x.IsCompleted).ToList();

            if (redisClientsLoaded.Count != _redisClients.Count)
            {
                return await _redisClients.WhenAny(x => x.IsConnected)
                    ?? await Task.WhenAny(_redisClients).Unwrap().ConfigureAwait(false);
            }

            var orderByLeastOutstandingOperations = (await Task.WhenAll(_redisClients)).OrderBy(client => client.OutstandingOperations).ToList();
                
            var connectedClient = orderByLeastOutstandingOperations.FirstOrDefault(client => client.IsConnected);

            return connectedClient ?? orderByLeastOutstandingOperations.First();
        }

        private void SetConnectionCallbacks()
        {
            foreach (var redisClient in _redisClients)
            {
                redisClient.ContinueWith(task =>
                {
                    var client = task.Result;

                    if (client.OnConnectionFailed == null)
                    {
                        client.OnConnectionFailed = OnConnectionFailed;
                    }

                    if (client.OnConnectionEstablished == null)
                    {
                        client.OnConnectionEstablished = OnConnectionEstablished;
                    }
                });
            }
        }

        public async Task<RedisClient[]> GetAllRedisClients()
        {
            return await Task.WhenAll(_redisClients.Select(task => task));
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
