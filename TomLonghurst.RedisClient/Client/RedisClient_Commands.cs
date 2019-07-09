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
        private async Task Authorize()
        {
            var command = $"{Commands.Auth} {_redisClientConfig.Password}".ToRedisProtocol();
            await SendAndReceiveAsync(command, ExpectSuccess, CancellationToken.None);
        }
        
        private async Task SelectDb()
        {
            var command = $"{Commands.Select} {_redisClientConfig.Db}".ToRedisProtocol();
            await SendAndReceiveAsync(command, ExpectSuccess, CancellationToken.None);
        }

        public async Task<Pong> Ping()
        {
            var pingCommand = Commands.Ping.ToRedisProtocol();

            var sw = Stopwatch.StartNew();
            var pingResponse = await SendAndReceiveAsync(pingCommand, ExpectWord, CancellationToken.None);
            sw.Stop();
            
            return new Pong(sw.Elapsed, pingResponse);
        }

        public Task<bool> KeyExistsAsync(string key)
        {
            return KeyExistsAsync(key, CancellationToken.None);
        }

        public async Task<bool> KeyExistsAsync(string key,
            CancellationToken cancellationToken)
        {
            return await await RunWithTimeout(async delegate
            {
                var command = $"{Commands.Exists} {key}".ToRedisProtocol();
                return await SendAndReceiveAsync(command, ExpectNumber, cancellationToken);
            }, cancellationToken) == 1;
        }

        public Task<RedisValue<string>> StringGetAsync(string key)
        {
            return StringGetAsync(key, CancellationToken.None);
        }

        public async Task<RedisValue<string>> StringGetAsync(string key,
            CancellationToken cancellationToken)
        {
            return new RedisValue<string>(await await await RunWithTimeout(async delegate
                {
                    var command = $"{Commands.Get} {key}".ToRedisProtocol();
                    return await SendAndReceiveAsync(command, ExpectData, cancellationToken);
                }, cancellationToken));
        }

        public Task<IEnumerable<string>> StringGetAsync(IEnumerable<string> keys)
        {
            return StringGetAsync(keys, CancellationToken.None);
        }

        public async Task<IEnumerable<string>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken)
        {
            return await await await RunWithTimeout(async delegate
            {
                var keysAsString = string.Join(" ", keys);
                var command = $"{Commands.MGet} {keysAsString}".ToRedisProtocol();

                return await SendAndReceiveAsync(command, ExpectArray, cancellationToken);
            }, cancellationToken);
        }

        public Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions)
        {
            return StringSetAsync(key, value, timeToLiveInSeconds, awaitOptions, CancellationToken.None);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await await RunWithTimeout(async delegate
            {
                var command = $"{Commands.SetEx} {key} {timeToLiveInSeconds} {value}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await await task;
                }
            }, cancellationToken);
        }

        public Task StringSetAsync(string key, string value, AwaitOptions awaitOptions)
        {
            return StringSetAsync(key, value, awaitOptions, CancellationToken.None);
        }

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await await RunWithTimeout(async delegate
            {
                var command = $"{Commands.Set} {key} {value}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await await task;
                }
            }, cancellationToken);
        }

        public Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs,
            AwaitOptions awaitOptions)
        {
            return StringSetAsync(keyValuePairs, awaitOptions, CancellationToken.None);
        }

        public async Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await await RunWithTimeout(async delegate
            {
                var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));
                var command = $"{Commands.MSet} {keysAndPairs}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await await task;
                }
            }, cancellationToken);
        }

        public Task DeleteKeyAsync(string key,
            AwaitOptions awaitOptions)
        {
            return DeleteKeyAsync(key, awaitOptions, CancellationToken.None);
        }

        public Task DeleteKeyAsync(string key,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            return DeleteKeyAsync(new[] {key}, awaitOptions, cancellationToken);
        }

        public Task DeleteKeyAsync(IEnumerable<string> keys,
            AwaitOptions awaitOptions)
        {
            return DeleteKeyAsync(keys, awaitOptions, CancellationToken.None);
        }

        public async Task DeleteKeyAsync(IEnumerable<string> keys,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken)
        {
            await await RunWithTimeout(async delegate
            {
                var keysAsString = string.Join(" ", keys);
                var command = $"{Commands.Del} {keysAsString}".ToRedisProtocol();
                var task = SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);
                
                if (awaitOptions == AwaitOptions.AwaitCompletion)
                {
                    await await task;
                }
            }, cancellationToken);
        }

        private Task SetClientName()
        {
            return SetClientName(CancellationToken.None);
        }

        private async Task SetClientName(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async delegate
            {
                var command = $"{Commands.Client} {Commands.SetName} {Client}".ToRedisProtocol();
                await SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);
            }, cancellationToken);
        }
    }
}