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

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private string lastCommand;

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
            return new RedisValue<string>(await RunWithTimeout(async token =>
                {
                    var command = $"{Commands.Get} {key}".ToRedisProtocol();
                    return await SendAndReceiveAsync(command, ExpectData, token);
                }, cancellationToken).ConfigureAwait(false));
        }

        public async Task<IEnumerable<RedisValue<string>>> StringGetAsync(IEnumerable<string> keys)
        {
            return await StringGetAsync(keys, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<IEnumerable<RedisValue<string>>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            return await RunWithTimeout(async token =>
            {
                var keysAsString = string.Join(" ", keys);
                var command = $"{Commands.MGet} {keysAsString}".ToRedisProtocol();

                return await SendAndReceiveAsync(command, ExpectArray, token);
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions)
        {
            await StringSetAsync(key, value, timeToLiveInSeconds, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            CollateMultipleRequestsForPipelining(new Tuple<string, string, int>(key, value, timeToLiveInSeconds), _stringSetWithTtlQueue);

            await RunWithTimeout(async token =>
            {
                var tuples = await _stringSetWithTtlQueue.DequeueAll(_sendSemaphoreSlim, token);
            
                if (!tuples.Any())
                {
                    return;
                }
                
                var command = tuples.Select(tuple => $"{Commands.SetEx} {tuple.Item1} {tuple.Item3} {tuple.Item2}").ToFireAndForgetCommand();

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
            CollateMultipleRequestsForPipelining(new Tuple<string, string>(key, value), _stringSetQueue);

            await RunWithTimeout(async token =>
            {
                var tuples = await _stringSetQueue.DequeueAll(_sendSemaphoreSlim, token);

                if (!tuples.Any())
                {
                    return;
                }
                
                var command = tuples.Select(tuple => $"{Commands.Set} {tuple.Item1} {tuple.Item2}").ToFireAndForgetCommand();
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
        
        public async Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs, 
            int timeToLiveInSeconds, 
            AwaitOptions awaitOptions)
        {
            await StringSetAsync(keyValuePairs, timeToLiveInSeconds, awaitOptions, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs,
            int timeToLiveInSeconds, 
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                keyValuePairs = keyValuePairs.ToList();
                var keys = keyValuePairs.Select(k => k.Key).ToList();

                var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));
                
                var setCommand = $"{Commands.MSet} {keysAndPairs}".ToRedisProtocol();
                await SendAndReceiveAsync(setCommand, ExpectSuccess, token);

                var expireCommand = keys.Select(key => $"{Commands.Expire} {key} {timeToLiveInSeconds}").ToFireAndForgetCommand();
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
                var keysAsString = string.Join(" ", keys);
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