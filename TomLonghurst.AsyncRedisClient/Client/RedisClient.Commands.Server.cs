using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private ServerCommands _serverCommands;
        public ServerCommands Server => _serverCommands;
        public class ServerCommands
        {
            private readonly RedisClient _redisClient;

            internal ServerCommands(RedisClient redisClient)
            {
                _redisClient = redisClient;
            }
            
            public ValueTask<string> Info()
            {
                return Info(CancellationToken.None);
            }
        
            public async ValueTask<string> Info(CancellationToken cancellationToken)
            {
                return await  _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendOrQueueAsync(RedisEncodable.From(Commands.Info), _redisClient.DataResultProcessor, CancellationToken.None);
                }, cancellationToken);
            }

            public Task<int> DBSize()
            {
                return DBSize(CancellationToken.None);
            }

            public async Task<int> DBSize(CancellationToken cancellationToken)
            {
                return await _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendOrQueueAsync(RedisEncodable.From(Commands.DbSize), _redisClient.IntegerResultProcessor, CancellationToken.None);
                }, cancellationToken);
            }
        }
    }
}