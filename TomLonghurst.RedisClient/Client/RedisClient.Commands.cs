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
using TomLonghurst.RedisClient.Models.RequestModels;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private RedisClient()
        {
            _clusterCommands = new ClusterCommands(this);
            _serverCommands = new ServerCommands(this);
        }

        internal string LastCommand;

        private async ValueTask Authorize(CancellationToken cancellationToken)
        {
            var command = $"{Commands.Auth} {ClientConfig.Password}";
            await SendAndReceiveAsync(command, ExpectSuccess, cancellationToken, true);
        }

        private async ValueTask SelectDb(CancellationToken cancellationToken)
        {

            var command = $"{Commands.Select} {ClientConfig.Db}";
            await SendAndReceiveAsync(command, ExpectSuccess, cancellationToken, true);
        }

        public async Task<Pong> Ping()
        {
            return await Ping(CancellationToken.None);
        }

        public async Task<Pong> Ping(CancellationToken cancellationToken)
        {
            var pingCommand = Commands.Ping;

            var sw = Stopwatch.StartNew();
            var pingResponse = await SendAndReceiveAsync(pingCommand, ExpectWord, cancellationToken);
            sw.Stop();

            return new Pong(sw.Elapsed, pingResponse);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await KeyExistsAsync(key, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<bool> KeyExistsAsync(string key,
            CancellationToken cancellationToken)
        {
            var command = $"{Commands.Exists} {key}";
            return await SendAndReceiveAsync(command, ExpectInteger, cancellationToken) == 1;
        }

        public async Task<StringRedisValue> StringGetAsync(string key)
        {
            return await StringGetAsync(key, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<StringRedisValue> StringGetAsync(string key,
            CancellationToken cancellationToken)
        {
            var command = $"{Commands.Get} {key}";
            return new StringRedisValue(
                await SendAndReceiveAsync(command, ExpectData, cancellationToken)
            );
        }

        public async Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys)
        {
            return await StringGetAsync(keys, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<IEnumerable<StringRedisValue>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            var keysAsString = string.Join(" ", keys);
            var command = $"{Commands.MGet} {keysAsString}";

            return await SendAndReceiveAsync(command, ExpectArray, cancellationToken);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions);
        }

        private async Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds,
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(redisKeyValue, timeToLiveInSeconds, awaitOptions, CancellationToken.None)
                .ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions, cancellationToken);
        }

        private async Task StringSetAsync(RedisKeyValue redisKeyValue, int timeToLiveInSeconds,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            var command = $"{Commands.SetEx} {redisKeyValue.Key} {timeToLiveInSeconds} {redisKeyValue.Value}";

            var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);

            if (awaitOptions == AwaitOptions.AwaitCompletion)
            {
                await task;
            }
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions);
        }

        private async Task StringSetAsync(RedisKeyValue redisKeyValue, AwaitOptions awaitOptions)
        {
            await StringSetAsync(redisKeyValue, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await StringSetAsync(new RedisKeyValue(key, value), awaitOptions, cancellationToken);
        }

        private async Task StringSetAsync(RedisKeyValue redisKeyValue, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            var command = $"{Commands.Set} {redisKeyValue.Key} {redisKeyValue.Value}";
            var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);

            if (awaitOptions == AwaitOptions.AwaitCompletion)
            {
                await task;
            }
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
            var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));

            var command = $"{Commands.MSet} {keysAndPairs}";
            var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);

            if (awaitOptions == AwaitOptions.AwaitCompletion)
            {
                await task;
            }
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            int timeToLiveInSeconds,
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(keyValuePairs, timeToLiveInSeconds, awaitOptions, CancellationToken.None)
                .ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<RedisKeyValue> keyValuePairs,
            int timeToLiveInSeconds,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {

            keyValuePairs = keyValuePairs.ToList();
            var keys = keyValuePairs.Select(k => k.Key).ToList();

            var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));

            var setCommand = $"{Commands.MSet} {keysAndPairs}";
            await SendAndReceiveAsync(setCommand, ExpectSuccess, cancellationToken);

            var expireCommand = keys.Select(key => $"{Commands.Expire} {key} {timeToLiveInSeconds}")
                .ToFireAndForgetCommand();
            var expireTask = SendAndReceiveAsync(expireCommand, ExpectSuccess, cancellationToken);

            if (awaitOptions == AwaitOptions.AwaitCompletion)
            {
                await expireTask;
            }
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
            var keysAsString = string.Join(" ", keys);
            var command = $"{Commands.Del} {keysAsString}";
            var task = SendAndReceiveAsync(command, ExpectInteger, cancellationToken);

            if (awaitOptions == AwaitOptions.AwaitCompletion)
            {
                await task;
            }
        }

        private async ValueTask SetClientNameAsync(CancellationToken cancellationToken)
        {
            var command = $"{Commands.Client} {Commands.SetName} {ClientConfig.ClientName}";
            await SendAndReceiveAsync(command, ExpectSuccess, cancellationToken, true);

        }

        public ValueTask<int> IncrementAsync(string key)
        {
            return IncrementAsync(key, CancellationToken.None);
        }

        public async ValueTask<int> IncrementAsync(string key, CancellationToken cancellationToken)
        {
            var command = $"{Commands.Incr} {key}";
            return await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask<int> IncrementByAsync(string key, int amount)
        {
            return IncrementByAsync(key, amount, CancellationToken.None);
        }

        public async ValueTask<int> IncrementByAsync(string key, int amount, CancellationToken cancellationToken)
        {
            var command = $"{Commands.IncrBy} {key} {amount}";
            return await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask<float> IncrementByAsync(string key, float amount)
        {
            return IncrementByAsync(key, amount, CancellationToken.None);
        }

        public async ValueTask<float> IncrementByAsync(string key, float amount, CancellationToken cancellationToken)
        {
            var command = $"{Commands.IncrByFloat} {key} {amount}";
            return await SendAndReceiveAsync(command, ExpectFloat, cancellationToken);
        }

        public ValueTask<int> DecrementAsync(string key)
        {
            return DecrementAsync(key, CancellationToken.None);
        }

        public async ValueTask<int> DecrementAsync(string key, CancellationToken cancellationToken)
        {
            var command = $"{Commands.Decr} {key}";
            return await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask<int> DecrementByAsync(string key, int amount)
        {
            return DecrementByAsync(key, amount, CancellationToken.None);
        }

        public async ValueTask<int> DecrementByAsync(string key, int amount, CancellationToken cancellationToken)
        {
            var command = $"{Commands.DecrBy} {key} {amount}";
            return await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask ExpireAsync(string key, int seconds)
        {
            return ExpireAsync(key, seconds, CancellationToken.None);
        }

        public async ValueTask ExpireAsync(string key, int seconds, CancellationToken cancellationToken)
        {
            var command = $"{Commands.Expire} {key} {seconds}";
            await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask ExpireAtAsync(string key, DateTimeOffset dateTime)
        {
            return ExpireAtAsync(key, dateTime, CancellationToken.None);
        }

        public async ValueTask ExpireAtAsync(string key, DateTimeOffset dateTime, CancellationToken cancellationToken)
        {
            var command = $"{Commands.ExpireAt} {key} {dateTime.ToUnixTimeSeconds()}";
            await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask PersistAsync(string key)
        {
            return PersistAsync(key, CancellationToken.None);
        }

        public async ValueTask PersistAsync(string key, CancellationToken cancellationToken)
        {
            var command = $"{Commands.Persist} {key}";
            await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }

        public ValueTask<int> TimeToLiveAsync(string key)
        {
            return TimeToLiveAsync(key, CancellationToken.None);
        }

        public async ValueTask<int> TimeToLiveAsync(string key, CancellationToken cancellationToken)
        {
            var command = $"{Commands.Ttl} {key}";
            return await SendAndReceiveAsync(command, ExpectInteger, cancellationToken);
        }
    }
}