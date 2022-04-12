using TomLonghurst.AsyncRedisClient.Client;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public class LuaScript
    {
        private readonly RedisClient _redisClient;
        private readonly string _hash;

        internal LuaScript(RedisClient redisClient, string hash)
        {
            _redisClient = redisClient;
            _hash = hash;
        }

        public Task<RawResult> ExecuteAsync(IReadOnlyList<string> keys, IEnumerable<string> arguments)
        {
            return ExecuteAsync(keys, arguments, CancellationToken.None);
        }

        public Task<RawResult> ExecuteAsync(IReadOnlyList<string> keys, IEnumerable<string> arguments, CancellationToken cancellationToken)
        {
            return _redisClient.Scripts.EvalSha(_hash, keys, arguments, cancellationToken);
        }
    }
}