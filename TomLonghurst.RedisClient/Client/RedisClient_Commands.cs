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
    public partial class RedisClient
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
            var pingResponse = await SendAndReceiveAsync(pingCommand, ExpectWord);
            sw.Stop();
            
            return new Pong(sw.Elapsed, pingResponse);
        }

        public async Task<bool> KeyExistsAsync(string key,
            CancellationToken cancellationToken = default)
        {
            return await await RunWithTimeout(async delegate
            {
                var command = $"{Commands.Exists} {key}".ToRedisProtocol();
                return await SendAndReceiveAsync(command, ExpectNumber, cancellationToken);
            }, cancellationToken) == 1;
        }

        public async Task<RedisValue<string>> StringGetAsync(string key,
            CancellationToken cancellationToken = default)
        {
            return new RedisValue<string>(await await await RunWithTimeout(async delegate
                {
                    var command = $"{Commands.Get} {key}".ToRedisProtocol();
                    return await SendAndReceiveAsync(command, ExpectData, cancellationToken);
                }, cancellationToken));
        }
        
        public async Task<IEnumerable<string>> StringGetAsync(IEnumerable<string> keys,
            CancellationToken cancellationToken = default)
        {
            return await await await RunWithTimeout(async delegate
            {
                var keysAsString = string.Join(" ", keys);
                var command = $"{Commands.MGet} {keysAsString}".ToRedisProtocol();

                return await SendAndReceiveAsync(command, ExpectArray, cancellationToken);
            }, cancellationToken);
        }

        public async Task StringSetAsync(string key, string value, int timeToLiveInSeconds, AwaitOptions awaitOptions,
            CancellationToken cancellationToken = default)
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

        public async Task StringSetAsync(string key, string value, AwaitOptions awaitOptions,
            CancellationToken cancellationToken = default)
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

        public async Task StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken = default)
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
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken = default)
        {
            return DeleteKeyAsync(new[] {key}, awaitOptions, cancellationToken);
        }

        public async Task DeleteKeyAsync(IEnumerable<string> keys,
            AwaitOptions awaitOptions,
            CancellationToken cancellationToken = default)
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

        private async Task SetClientName(CancellationToken cancellationToken = default)
        {
            await RunWithTimeout(async delegate
            {
                var command = $"{Commands.Client} {Commands.SetName} {Client}".ToRedisProtocol();
                await SendAndReceiveAsync(command, ExpectSuccess, cancellationToken);
            }, cancellationToken);
        }
    }
}