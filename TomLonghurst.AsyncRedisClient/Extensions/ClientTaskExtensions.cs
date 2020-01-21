using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Compression;
using TomLonghurst.AsyncRedisClient.Models;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class ClientTaskExtensions
    {
        
        // TODO Finish this
        
        public static Task<StringRedisValue> StringGetAsync(this Task<RedisClient> redisClient, string key, ICompression compression)
        {
            return StringGetAsync(redisClient, key, compression, CancellationToken.None);
        }

        public static async Task<StringRedisValue> StringGetAsync(this Task<RedisClient> redisClient, string key, ICompression compression, CancellationToken cancellationToken)
        {
            var client = await GetClient(redisClient);
            return await client.StringGetAsync(key, compression, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<RedisClient> GetClient(Task<RedisClient> redisClient)
        {
            return await redisClient.ConfigureAwait(false);
        }
    }
}