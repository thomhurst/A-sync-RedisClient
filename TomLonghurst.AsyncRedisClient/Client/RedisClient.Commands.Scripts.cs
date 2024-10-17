using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Client;

public partial class RedisClient : IDisposable
{
    public ScriptCommands Scripts { get; }

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
            var command = RedisCommand.From(Commands.Script, Commands.Load, script);
            var scriptResponse = await _redisClient.RunWithTimeout(async token =>
            {
                return await _redisClient.SendOrQueueAsync(command, _redisClient.DataResultProcessor, token);
            }, cancellationToken);
                
            return new LuaScript(_redisClient, scriptResponse);
        }

        internal async Task<RawResult> EvalSha(string sha1Hash, IEnumerable<string> keys, IEnumerable<string> arguments, CancellationToken cancellationToken)
        {
            // TODO:
            return default;
            // var keysList = keys.ToList();
            // var command = RedisCommand.FromScript(Commands.EvalSha, sha1Hash.ToRedisEncoded(), keysList, arguments);
            //
            // var scriptResult = await _redisClient.RunWithTimeout(async token =>
            //     {
            //         return await _redisClient.SendOrQueueAsync(command, _redisClient.GenericResultProcessor, token);
            //     },
            //     cancellationToken);
            //     
            // return scriptResult;
        }
    }
}