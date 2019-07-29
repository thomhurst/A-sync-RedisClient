using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Constants;
using TomLonghurst.RedisClient.Enums;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Models.RequestModels;
using TomLonghurst.RedisClient.Pipes;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private RedisClient()
        {
            Options = new Lazy<Tuple<PipeOptions, PipeOptions>>(() =>
            {
                const int DefaultMinimumSegmentSize = 1024 * 8;

                const long Receive_ResumeWriterThreshold = DefaultMinimumSegmentSize * 1024 * 16 / 2;
                const long Receive_PauseWriterThreshold = DefaultMinimumSegmentSize * 1024 * 16;
                
                const long Send_PauseWriterThreshold = 512 * 1024;
                const long Send_ResumeWriterThreshold = Send_PauseWriterThreshold / 2;
                
                var pipeScheduler = new PipeThreadPoolScheduler(workerCount: 5);
                var defaultPipeOptions = PipeOptions.Default;

                var receivePipeOptions = new PipeOptions(
                    defaultPipeOptions.Pool,
                    pipeScheduler,
                    pipeScheduler,
                    Receive_PauseWriterThreshold,
                    Receive_ResumeWriterThreshold,
                    DefaultMinimumSegmentSize,
                    false);
                
                var sendPipeOptions = new PipeOptions(
                    defaultPipeOptions.Pool,
                    pipeScheduler,
                    pipeScheduler,
                    Send_PauseWriterThreshold,
                    Send_ResumeWriterThreshold,
                    DefaultMinimumSegmentSize,
                    false);

                return new Tuple<PipeOptions, PipeOptions>(sendPipeOptions, receivePipeOptions);
            });

            _clusterCommands = new ClusterCommands(this);
            _serverCommands = new ServerCommands(this);
        }

        internal string LastCommand;

        private async ValueTask Authorize(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = $"{Commands.Auth} {ClientConfig.Password}";
                await SendAndReceiveAsync(command, ExpectSuccess, CancellationToken.None, true);
            }, cancellationToken);
        }
        
        private async ValueTask SelectDb(CancellationToken cancellationToken)
        {
            await RunWithTimeout(async token =>
            {
                var command = $"{Commands.Select} {ClientConfig.Db}";
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
                var command = $"{Commands.Exists} {key}";
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
                    var command = $"{Commands.Get} {key}";
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
                var keysAsString = string.Join(" ", keys);
                var command = $"{Commands.MGet} {keysAsString}";

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
                var command = $"{Commands.SetEx} {redisKeyValue.Key} {timeToLiveInSeconds} {redisKeyValue.Value}";

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
                var command = $"{Commands.Set} {redisKeyValue.Key} {redisKeyValue.Value}";
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
                var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));
                
                var command = $"{Commands.MSet} {keysAndPairs}";
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
                keyValuePairs = keyValuePairs.ToList();
                var keys = keyValuePairs.Select(k => k.Key).ToList();
                
                var keysAndPairs = string.Join(" ", keyValuePairs.Select(pair => $"{pair.Key} {pair.Value}"));
                
                var setCommand = $"{Commands.MSet} {keysAndPairs}";
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
                var command = $"{Commands.Del} {keysAsString}";
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
                var command = $"{Commands.Client} {Commands.SetName} {ClientConfig.ClientName}";
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
                var command = $"{Commands.Incr} {key}";
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
                var command = $"{Commands.IncrBy} {key} {amount}";
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
                var command = $"{Commands.IncrByFloat} {key} {amount}";
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
                var command = $"{Commands.Decr} {key}";
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
                var command = $"{Commands.DecrBy} {key} {amount}";
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
                var command = $"{Commands.Expire} {key} {seconds}";
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
                var command = $"{Commands.ExpireAt} {key} {dateTime.ToUnixTimeSeconds()}";
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
                var command = $"{Commands.Persist} {key}";
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
                var command = $"{Commands.Ttl} {key}";
                return await SendAndReceiveAsync(command, ExpectInteger, token);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}