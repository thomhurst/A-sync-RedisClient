using System.Diagnostics;
using System.Text;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.RequestModels;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        internal ReadOnlyMemory<byte> LastCommand;

        private async ValueTask Authorize(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Auth, ClientConfig.Password.AsReadOnlyByteMemory());
                await SendOrQueueAsync(command, SuccessResultProcessor, CancellationToken.None, true);
            }, cancellationToken);
        }
        
        private async ValueTask SelectDb(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Select, ClientConfig.Db.AsReadOnlyByteMemory());
                await SendOrQueueAsync(command, SuccessResultProcessor, CancellationToken.None, true);
            }, cancellationToken);
        }

        public Task<Pong> Ping()
        {
            return Ping(CancellationToken.None);
        }

        public async Task<Pong> Ping(CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var pingCommand = Commands.Ping;

                var sw = Stopwatch.StartNew();
                var pingResponse = await SendOrQueueAsync(pingCommand, WordResultProcessor, CancellationToken.None);
                sw.Stop();

                return new Pong(sw.Elapsed, pingResponse);
            }, cancellationToken);
        }

        public Task<bool> KeyExistsAsync(string key)
        {
            return KeyExistsAsync(key, CancellationToken.None);
        }

        public async Task<bool> KeyExistsAsync(string key,
            CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Exists, key.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false) == 1;
        }

        public Task<StringRedisValue> StringGetAsync(string key)
        {
            return StringGetAsync(key, CancellationToken.None);
        }

        public async Task<StringRedisValue> StringGetAsync(string key,
            CancellationToken cancellationToken)
        {
            return new StringRedisValue(await RunWithTimeout(async token =>
                {
                    var command = RedisEncoder.EncodeCommand(Commands.Get, key.AsReadOnlyByteMemory());
                    return await SendOrQueueAsync(command, DataResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false));
        }

        public Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys)
        {
            return StringGetAsync(keys, CancellationToken.None);
        }

        public async Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.MGet, keys.Select(x => x.AsReadOnlyByteMemory()).ToArray());

                return await SendOrQueueAsync(command, ArrayResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task StringSetAsync(string key, string value, int timeToLiveInSeconds)
        {
            return StringSetAsync(new RedisKeyValue(key, value), timeToLiveInSeconds);
        }

        private Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds)
        {
            return StringSetAsync(redisKeyValue, timeToLiveInSeconds, CancellationToken.None);
        }

        public Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            CancellationToken cancellationToken)
        {
            return StringSetAsync(new RedisKeyValue(key, value), cancellationToken);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds, 
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.SetEx, redisKeyValue.Key.AsReadOnlyByteMemory(), timeToLiveInSeconds.AsReadOnlyByteMemory(), redisKeyValue.Value.AsReadOnlyByteMemory());
               
                await SendOrQueueAsync(command, SuccessResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task StringSetAsync(string key, string value)
        {
            return StringSetAsync(new RedisKeyValue(key, value));
        }
        
        private Task StringSetAsync(RedisKeyValue redisKeyValue)
        {
            return StringSetAsync(redisKeyValue, CancellationToken.None);
        }

        public Task StringSetAsync(string key, string value, CancellationToken cancellationToken)
        {
            return StringSetAsync(new RedisKeyValue(key, value), cancellationToken);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Set, redisKeyValue.Key.AsReadOnlyByteMemory(), redisKeyValue.Value.AsReadOnlyByteMemory());
                await SendOrQueueAsync(command, SuccessResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs)
        {
            return StringSetAsync(keyValuePairs, CancellationToken.None);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.MSet, keyValuePairs.SelectMany(x =>
                {
                    var arr = new ReadOnlyMemory<byte>[2];
                    arr[0] = x.Key.AsReadOnlyByteMemory();
                    arr[1] = x.Value.AsReadOnlyByteMemory();
                    return arr;
                }).ToArray());
                
                await SendOrQueueAsync(command, SuccessResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }
        
        public Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs, 
            int timeToLiveInSeconds)
        {
            return StringSetAsync(keyValuePairs, timeToLiveInSeconds, CancellationToken.None);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            int timeToLiveInSeconds,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var redisKeyValues = keyValuePairs.ToList();

                var keys = redisKeyValues.Select(value => value.Key).ToList();
                var arguments = new List<string> { timeToLiveInSeconds.ToString() }.Concat(redisKeyValues.Select(value => value.Value));

                if (Scripts.MultiSetexScript == null)
                {
                    Scripts.MultiSetexScript = await Scripts.LoadScript(
                        "for i=1, #KEYS do " +
                        "redis.call(\"SETEX\", KEYS[i], ARGV[1], ARGV[i+1]); " +
                        "end",
                        cancellationToken);
                }

                await Scripts.MultiSetexScript.ExecuteAsync(keys, arguments, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task DeleteKeyAsync(string key)
        {
            return DeleteKeyAsync(key, CancellationToken.None);
        }

        public async Task DeleteKeyAsync(string key,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = RedisEncoder.EncodeCommand(Commands.Del, key.AsReadOnlyByteMemory());
                await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task DeleteKeyAsync(IEnumerable<string> keys)
        {
            return DeleteKeyAsync(keys, CancellationToken.None);
        }

        public async Task DeleteKeyAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = RedisEncoder.EncodeCommand(Commands.Del, keys.Select(x => x.AsReadOnlyByteMemory()).ToArray());
                await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask SetClientNameAsync(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = RedisEncoder.EncodeCommand(Commands.Client, Commands.SetName, ClientConfig.ClientName.AsReadOnlyByteMemory());
                await SendOrQueueAsync(command, SuccessResultProcessor, token, true);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<int> IncrementAsync(string key)
        {
            return IncrementAsync(key, CancellationToken.None);
        }

        public async ValueTask<int> IncrementAsync(string key, CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Incr, key.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<int> IncrementByAsync(string key, int amount)
        {
            return IncrementByAsync(key, amount, CancellationToken.None);
        }

        public async ValueTask<int> IncrementByAsync(string key, int amount, CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.IncrBy, key.AsReadOnlyByteMemory(), amount.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<float> IncrementByAsync(string key, float amount)
        {
            return IncrementByAsync(key, amount, CancellationToken.None);
        }

        public async ValueTask<float> IncrementByAsync(string key, float amount, CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.IncrByFloat, key.AsReadOnlyByteMemory(), amount.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, FloatResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<int> DecrementAsync(string key)
        {
            return DecrementAsync(key, CancellationToken.None);
        }

        public async ValueTask<int> DecrementAsync(string key, CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Decr, key.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<int> DecrementByAsync(string key, int amount)
        {
            return DecrementByAsync(key, amount, CancellationToken.None);
        }

        public async ValueTask<int> DecrementByAsync(string key, int amount, CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.DecrBy, key.AsReadOnlyByteMemory(), amount.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask ExpireAsync(string key, int seconds)
        {
            return ExpireAsync(key, seconds, CancellationToken.None);
        }

        public async ValueTask ExpireAsync(string key, int seconds, CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Expire, key.AsReadOnlyByteMemory(), seconds.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask ExpireAsync(IEnumerable<string> keys,
            int timeToLiveInSeconds)
        {
            return ExpireAsync(keys, timeToLiveInSeconds, new CancellationToken());
        }

        public async ValueTask ExpireAsync(IEnumerable<string> keys,
            int timeToLiveInSeconds, 
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var arguments = new List<string> { timeToLiveInSeconds.ToString() };

                if (Scripts.MultiExpireScript == null)
                {
                    Scripts.MultiExpireScript = await Scripts.LoadScript(
                        "for i, name in ipairs(KEYS) do redis.call(\"EXPIRE\", name, ARGV[1]); end",
                        cancellationToken);
                }

                await Scripts.MultiExpireScript.ExecuteAsync(keys, arguments, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask ExpireAtAsync(string key, DateTimeOffset dateTime)
        {
            return ExpireAtAsync(key, dateTime, CancellationToken.None);
        }

        public async ValueTask ExpireAtAsync(string key, DateTimeOffset dateTime, CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.ExpireAt, key.AsReadOnlyByteMemory(), dateTime.ToUnixTimeSeconds().AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask PersistAsync(string key)
        {
            return PersistAsync(key, CancellationToken.None);
        }

        public async ValueTask PersistAsync(string key, CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Persist, key.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<int> TimeToLiveAsync(string key)
        {
            return TimeToLiveAsync(key, CancellationToken.None);
        }

        public async ValueTask<int> TimeToLiveAsync(string key, CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisEncoder.EncodeCommand(Commands.Ttl, key.AsReadOnlyByteMemory());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}