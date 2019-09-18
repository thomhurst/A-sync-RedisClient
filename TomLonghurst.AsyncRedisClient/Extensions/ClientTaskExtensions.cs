using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Models;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class ClientTaskExtensions
    {
        
        // TODO Finish this
        
        public static Task<StringRedisValue> StringGetAsync(this Task<RedisClient> redisClient, string key)
        {
            return StringGetAsync(redisClient, key, CancellationToken.None);
        }

        public static async Task<StringRedisValue> StringGetAsync(this Task<RedisClient> redisClient, string key, CancellationToken cancellationToken)
        {
            var client = await redisClient;
            return await client.StringGetAsync(key, cancellationToken);
        }
    }
}