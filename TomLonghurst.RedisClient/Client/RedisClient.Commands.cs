using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Constants;
using TomLonghurst.RedisClient.Enums;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private async ValueTask Authorize(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = $"{Commands.Auth} {_redisClientConfig.Password}".ToRedisProtocol();
                await SendAndReceiveAsync(command, ExpectSuccess, CancellationToken.None, true);
            }, cancellationToken);
        }
        
        private async ValueTask SelectDb(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = $"{Commands.Select} {_redisClientConfig.Db}".ToRedisProtocol();
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
                var pingCommand = Commands.Ping.ToRedisProtocol();

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
                var command = $"{Commands.Exists} {key}".ToRedisProtocol();
                return await SendAndReceiveAsync(command, ExpectNumber, token);
            }, cancellationToken).ConfigureAwait(false) == 1;
        }

        public async Task<RedisValue<string>> StringGetAsync(string key)
        {
            return await StringGetAsync(key, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<RedisValue<string>> StringGetAsync(string key,
            CancellationToken cancellationToken)
        {
            var localCache = _manager.GetCache<RedisValue<string>>(key);
            if (localCache != null && localCache.HasValue)
            {
                return localCache;
            }
            
            return new RedisValue<string>(await RunWithTimeout(async token =>
                {
                    var command = $"{Commands.Get} {key}".ToRedisProtocol();
                    return await SendAndReceiveAsync(command, ExpectData, token);
                }, cancellationToken).ConfigureAwait(false))
                .Also(delegate(RedisValue<string> redisValue) { _manager.SetCache(redisValue.Key, redisValue.Value); });
        }

        public async Task<IList<RedisValue<string>>> StringGetAsync(IEnumerable<string> keys)
        {
            return await StringGetAsync(keys, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<IList<RedisValue<string>>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            keys = keys.ToList();

            var localCache = keys.Select(key => _manager.GetCache<RedisValue<string>>(key)).ToList();
            if (localCache.Count(x => x != null && x.HasValue) == keys.Count())
            {
                return localCache.ToList();
            }
            
            return (await RunWithTimeout(async token =>
                {
                    var keysAsString = string.Join(" ", keys);
                    var command = $"{Commands.MGet} {keysAsString}".ToRedisProtocol();

                    return await SendAndReceiveAsync(command, ExpectArray, token);
                }, cancellationToken).ConfigureAwait(false))
                .Also(delegate(IList<RedisValue<string>> list) { AddKeysToValuesResponse((IList<string>) keys, list); })
                .Also(delegate(IList<RedisValue<string>> list) {
                    foreach (var redisValue in list)
                    {
                        if (redisValue.HasValue)
                        {
                            _manager.SetCache(redisValue.Key, redisValue.Value);
                        }
                    }
                });
        }

        private void AddKeysToValuesResponse<T>(IList<string> keys, IList<RedisValue<T>> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                AddKeyToValueResponse(keys[i], values[i]);
            }
        }

        private void AddKeyToValueResponse<T>(string key, RedisValue<T> value)
        {
            value.Key = key;
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions)
        {
            await StringSetAsync(key, value, timeToLiveInSeconds, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = $"{Commands.SetEx} {key} {timeToLiveInSeconds} {value}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions)
        {
            await StringSetAsync(key, value, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = $"{Commands.Set} {key} {value}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs,
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(keyValuePairs, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));
                var command = $"{Commands.MSet} {keysAndPairs}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
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
            var keysList = keys.ToList();
            foreach (var key in keysList)
            {
                _manager.DeleteCache(key);
            }

            await RunWithTimeout(async token => 
            {
                var keysAsString = string.Join(" ", keysList);
                var command = $"{Commands.Del} {keysAsString}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, token);

                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await task;
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask SetClientName(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token => 
            {
                var command = $"{Commands.Client} {Commands.SetName} {_redisClientConfig.ClientName}".ToRedisProtocol();
                await SendAndReceiveAsync(command, ExpectSuccess, token, true);
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> ClusterInfo()
        {
            return await ClusterInfo(CancellationToken.None).ConfigureAwait(false);
        }
        
        public async Task<string> ClusterInfo(CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var command = Commands.ClusterInfo.ToRedisProtocol();
                return await SendAndReceiveAsync(command, ExpectData, token);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}