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
            var client = await GetClient(redisClient);
            return await client.StringGetAsync(key, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<RedisClient> GetClient(Task<RedisClient> redisClient)
        {
            return await redisClient.ConfigureAwait(false);
        }
    }
}