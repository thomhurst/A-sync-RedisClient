using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Client;

public partial class RedisClient : IDisposable
{
    private ScriptCommands? _scriptCommands;
    public ScriptCommands? Scripts => _scriptCommands;
    public class ScriptCommands
    {
        private readonly RedisClient _redisClient;

        internal LuaScript? MultiExpireScript;
        internal LuaScript? MultiSetexScript;

        internal ScriptCommands(RedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        public async Task FlushScripts(CancellationToken cancellationToken)
        {
            await _redisClient.RunWithTimeout(async token =>
            {
                await _redisClient.SendOrQueueAsync(Commands.ScriptFlush, _redisClient.SuccessResultProcessor, token);
            }, cancellationToken);
        }

        public async Task<LuaScript> LoadScript(string script, CancellationToken cancellationToken)
        {
            var command = RedisCommand.From(Commands.Script, Commands.Load, script.ToRedisEncoded());
            var scriptResponse = await _redisClient.RunWithTimeout(async token =>
            {
                return await _redisClient.SendOrQueueAsync(command, _redisClient.DataResultProcessor, token);
            }, cancellationToken);
                
            return new LuaScript(_redisClient, scriptResponse);
        }

        internal async Task<RawResult> EvalSha(string sha1Hash, IEnumerable<string> keys, IEnumerable<string> arguments, CancellationToken cancellationToken)
        {
            var keysList = keys.ToList();
            var command = RedisCommand.FromScript(Commands.EvalSha, sha1Hash.ToRedisEncoded(), keysList, arguments);

            var scriptResult = await _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendOrQueueAsync(command, _redisClient.GenericResultProcessor, token);
                },
                cancellationToken);
                
            return scriptResult;
        }
    }
}