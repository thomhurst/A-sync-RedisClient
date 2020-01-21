using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Compression;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Models.RequestModels;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        internal IRedisCommand LastCommand;

        private async ValueTask Authorize(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Auth, ClientConfig.Password.ToRedisEncoded());
                await SendOrQueueAsync(command, SuccessResultProcessor, CancellationToken.None, true);
            }, cancellationToken);
        }
        
        private async ValueTask SelectDb(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Select, ClientConfig.Db.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.Exists, key.ToRedisEncoded());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false) == 1;
        }

        public Task<StringRedisValue> StringGetAsync(string key, ICompression compression)
        {
            return StringGetAsync(key, compression, CancellationToken.None);
        }

        public async Task<StringRedisValue> StringGetAsync(string key, ICompression compression,
            CancellationToken cancellationToken)
        {
            var value = await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Get, key.ToRedisEncoded());
                return await SendOrQueueAsync(command, DataResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
            
            return new StringRedisValue(await compression.DecompressAsync(value));
        }

        public Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys, ICompression compression)
        {
            return StringGetAsync(keys, compression, CancellationToken.None);
        }

        public async Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys, ICompression compression,
            CancellationToken cancellationToken)
        {
            var values = await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.MGet, keys.ToRedisEncoded());

                return await SendOrQueueAsync(command, ArrayResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
            
            return await Task.WhenAll(values.Select(async v => new StringRedisValue(await compression.DecompressAsync(v))));
        }

        public Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            ICompression compression)
        {
            return StringSetAsync(new RedisKeyValue(key, value), timeToLiveInSeconds, compression);
        }

        private Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds, ICompression compression)
        {
            return StringSetAsync(redisKeyValue, timeToLiveInSeconds, compression, CancellationToken.None);
        }

        public Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            ICompression compression,
            CancellationToken cancellationToken)
        {
            return StringSetAsync(new RedisKeyValue(key, value), timeToLiveInSeconds, compression, cancellationToken);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds,
            ICompression compression,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var value = await compression.CompressAsync(redisKeyValue.Value);
                
                var command = RedisCommand.From(Commands.SetEx, redisKeyValue.Key.ToRedisEncoded(),
                    timeToLiveInSeconds.ToRedisEncoded(), value.ToRedisEncoded());
               
                await SendOrQueueAsync(command, SuccessResultProcessor, token);
                
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task StringSetAsync(string key, string value, ICompression compression)
        {
            return StringSetAsync(new RedisKeyValue(key, value), compression);
        }
        
        private Task StringSetAsync(RedisKeyValue redisKeyValue, ICompression compression)
        {
            return StringSetAsync(redisKeyValue, compression, CancellationToken.None);
        }

        public Task StringSetAsync(string key, string value, ICompression compression, CancellationToken cancellationToken)
        {
            return StringSetAsync(new RedisKeyValue(key, value), compression, cancellationToken);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, ICompression compression,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var value = await compression.CompressAsync(redisKeyValue.Value);
                var command = RedisCommand.From(Commands.Set, redisKeyValue.Key.ToRedisEncoded(),
                    value.ToRedisEncoded());
                await SendOrQueueAsync(command, SuccessResultProcessor, token);
                
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            ICompression compression)
        {
            return StringSetAsync(keyValuePairs, compression, CancellationToken.None);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            ICompression compression,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var encodedKeysAndValues = new List<IRedisEncodable>();
                foreach (var keyValuePair in keyValuePairs)
                {
                    encodedKeysAndValues.Add(keyValuePair.Key.ToRedisEncoded());
                    var value = await compression.CompressAsync(keyValuePair.Value);
                    encodedKeysAndValues.Add(value.ToRedisEncoded());
                }
                
                var command = RedisCommand.From(Commands.MSet, encodedKeysAndValues);
                await SendOrQueueAsync(command, SuccessResultProcessor, token);
                
            }, cancellationToken).ConfigureAwait(false);
        }
        
        public Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs, 
            int timeToLiveInSeconds, 
            ICompression compression)
        {
            return StringSetAsync(keyValuePairs, timeToLiveInSeconds, compression, CancellationToken.None);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            int timeToLiveInSeconds, 
            ICompression compression,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var redisKeyValues = keyValuePairs.ToList();

                var keys = redisKeyValues.Select(value => value.Key).ToList();
                var compressedRedisKeys = await Task.WhenAll(redisKeyValues.Select(async value => await compression.CompressAsync(value.Value)));
                var arguments = new List<string> { timeToLiveInSeconds.ToString() }.Concat(compressedRedisKeys);

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

        public Task DeleteKeyAsync(string key,
            CancellationToken cancellationToken)
        {
            return DeleteKeyAsync(new[] { key }, cancellationToken);
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
                var command = RedisCommand.From(Commands.Del, keys.ToRedisEncoded());
                await SendOrQueueAsync(command, IntegerResultProcessor, token);
                
            }, cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask SetClientNameAsync(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = RedisCommand.From(Commands.Client, Commands.SetName, ClientConfig.ClientName.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.Incr, key.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.IncrBy, key.ToRedisEncoded(), amount.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.IncrByFloat, key.ToRedisEncoded(), amount.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.Decr, key.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.DecrBy, key.ToRedisEncoded(), amount.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.Expire, key.ToRedisEncoded(), seconds.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.ExpireAt, key.ToRedisEncoded(), dateTime.ToUnixTimeSeconds().ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.Persist, key.ToRedisEncoded());
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
                var command = RedisCommand.From(Commands.Ttl, key.ToRedisEncoded());
                return await SendOrQueueAsync(command, IntegerResultProcessor, token);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}