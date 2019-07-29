using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Constants;
using TomLonghurst.RedisClient.Enums;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Models.Commands;
using TomLonghurst.RedisClient.Models.RequestModels;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        internal IRedisCommand LastCommand;

        private async ValueTask Authorize(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Auth, ClientConfig.Password.ToRedisEncoded());
                await SendAndReceiveAsync(command, ExpectSuccess, CancellationToken.None, true);
            }, cancellationToken);
        }
        
        private async ValueTask SelectDb(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Select, ClientConfig.Db.ToRedisEncoded());
                await SendAndReceiveAsync(command, ExpectSuccess, CancellationToken.None, true);
            }, cancellationToken);
        }

        public async Task<Pong> Ping()
        {
            return await Ping(CancellationToken.None);
        }

        public async Task<Pong> Ping(CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var pingCommand = Commands.Ping;

                var sw = Stopwatch.StartNew();
                var pingResponse = await SendAndReceiveAsync(pingCommand, ExpectWord, CancellationToken.None);
                sw.Stop();

                return new Pong(sw.Elapsed, pingResponse);
            }, cancellationToken);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await KeyExistsAsync(key, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<bool> KeyExistsAsync(string key,
            CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Exists, key.ToRedisEncoded());
                return await SendAndReceiveAsync(command, ExpectInteger, token);
            }, cancellationToken).ConfigureAwait(false) == 1;
        }

        public async Task<StringRedisValue> StringGetAsync(string key)
        {
            return await StringGetAsync(key, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<StringRedisValue> StringGetAsync(string key,
            CancellationToken cancellationToken)
        {
            return new StringRedisValue(await RunWithTimeout(async token =>
                {
                    var command = RedisCommand.From(Commands.Get, key.ToRedisEncoded());
                    return await SendAndReceiveAsync(command, ExpectData, token);
                }, cancellationToken).ConfigureAwait(false));
        }

        public async Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys)
        {
            return await StringGetAsync(keys, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.MGet, keys.ToRedisEncoded());

                return await SendAndReceiveAsync(command, ExpectArray, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions);
        }

        private async Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds, AwaitOptions awaitOptions)
        {
            await StringSetAsync(redisKeyValue, timeToLiveInSeconds, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions, cancellationToken);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
           //CollateMultipleRequestsForPipelining(new Tuple<string, string, int>(key, value, timeToLiveInSeconds), _stringSetWithTtlQueue);

            await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.SetEx, redisKeyValue.Key.ToRedisEncoded(),
                    timeToLiveInSeconds.ToRedisEncoded(), redisKeyValue.Value.ToRedisEncoded());
               
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, AwaitOptions awaitOptions)
        {
            await StringSetAsync(redisKeyValue, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions, CancellationToken cancellationToken)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions, cancellationToken);
        }
        
        private async Task StringSetAsync(RedisKeyValue redisKeyValue, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            //CollateMultipleRequestsForPipelining(new Tuple<string, string>(key, value), _stringSetQueue);

            await RunWithTimeout(async token =>
            {
                var command = RedisCommand.From(Commands.Set, redisKeyValue.Key.ToRedisEncoded(),
                    redisKeyValue.Value.ToRedisEncoded());
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(keyValuePairs, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var encodedKeysAndValues = new List<IRedisEncodable>();
                foreach (var keyValuePair in keyValuePairs)
                {
                    encodedKeysAndValues.Add(keyValuePair.Key.ToRedisEncoded());
                    encodedKeysAndValues.Add(keyValuePair.Value.ToRedisEncoded());
                }
                
                var command = RedisCommand.From(Commands.MSet, encodedKeysAndValues);
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs, 
            int timeToLiveInSeconds, 
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(keyValuePairs, timeToLiveInSeconds, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            int timeToLiveInSeconds, 
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var encodedKeysAndValues = new List<IRedisEncodable>();
                foreach (var keyValuePair in keyValuePairs)
                {
                    encodedKeysAndValues.Add(keyValuePair.Key.ToRedisEncoded());
                    encodedKeysAndValues.Add(keyValuePair.Value.ToRedisEncoded());
                }
                
                var setCommand = RedisCommand.From(Commands.MSet, encodedKeysAndValues);
                await SendAndReceiveAsync(setCommand, ExpectSuccess, token);
                
                var expireCommand = keyValuePairs.Select(key => RedisCommand.From(Commands.Expire, key.ToRedisEncoded(), timeToLiveInSeconds.ToRedisEncoded())).ToFireAndForgetCommand();
                var expireTask = SendAndReceiveAsync(expireCommand, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await expireTask;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteKeyAsync(string key,
            AwaitOptions awaitOptions)
        {
            await DeleteKeyAsync(key, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task DeleteKeyAsync(string key,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await DeleteKeyAsync(new[] {key}, awaitOptions, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteKeyAsync(IEnumerable<string> keys,
            AwaitOptions awaitOptions)
        {
            await DeleteKeyAsync(keys, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task DeleteKeyAsync(IEnumerable<string> keys,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = RedisCommand.From(Commands.Del, keys.ToRedisEncoded());
                var task = SendAndReceiveAsync(command, ExpectInteger, token);

                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask SetClientNameAsync(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = RedisCommand.From(Commands.Client, Commands.SetName, ClientConfig.ClientName.ToRedisEncoded());
                await SendAndReceiveAsync(command, ExpectSuccess, token, true);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectFloat, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
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
                return await SendAndReceiveAsync(command, ExpectInteger, token);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}