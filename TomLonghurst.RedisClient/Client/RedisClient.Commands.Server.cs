using System;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private readonly ServerCommands _serverCommands;
        public ServerCommands Server => _serverCommands;
        public class ServerCommands
        {
            private readonly RedisClient _redisClient;

            internal ServerCommands(RedisClient redisClient)
            {
                _redisClient = redisClient;
            }
            
            public async ValueTask<string> Info()
            {
                return await Info(CancellationToken.None);
            }
        
            public async ValueTask<string> Info(CancellationToken cancellationToken)
            {
                return await _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendAndReceiveAsync(Commands.Info, _redisClient.ExpectData, CancellationToken.None, true);
                }, cancellationToken);
            }

            public async Task<int> DBSize()
            {
                return await DBSize(CancellationToken.None);
            }

            public async Task<int> DBSize(CancellationToken cancellationToken)
            {
                return await _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendAndReceiveAsync(Commands.DbSize, _redisClient.ExpectInteger, CancellationToken.None, true);
                }, cancellationToken);
            }
        }
    }
}