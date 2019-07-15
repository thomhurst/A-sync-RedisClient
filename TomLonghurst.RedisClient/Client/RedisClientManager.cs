using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientManager
    {
        private readonly List<Lazy<Task<RedisClient>>> _lazyRedisClients = new List<Lazy<Task<RedisClient>>>();
        private readonly MemoryCache _memoryCache = MemoryCache.Default;

        internal void SetCache(string key, object value)
        {
            if (value != null)
            {
                _memoryCache.Set(key, value, DateTimeOffset.Now.AddSeconds(15));
            }
        }
        
        internal T GetCache<T>(string key)
        {
            var obj = _memoryCache.Get(key);
            
            if (obj == null)
            {
                return default;
            }

            if (obj.GetType() == typeof(T) || obj.GetType().IsAssignableFrom(typeof(T)))
            {
                return (T) obj;
            }
            
            return default;
        }
        
        public RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
        {
            if (redisClientPoolSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
            }

            for (var i = 0; i < redisClientPoolSize; i++)
            {
                _lazyRedisClients.Add(new Lazy<Task<RedisClient>>(() => RedisClient.ConnectAsync(clientConfig, this)));
            }
        }

        public async Task<RedisClient> GetRedisClientAsync()
        {
            if (_lazyRedisClients.Count == 1)
            {
                return await _lazyRedisClients.First().Value;
            }
            
            var lazyClientNotYetLoaded = _lazyRedisClients.FirstOrDefault(lazy => !lazy.IsValueCreated);

            if(lazyClientNotYetLoaded != null)
            {
                return await lazyClientNotYetLoaded.Value;
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

        public void DeleteCache(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
