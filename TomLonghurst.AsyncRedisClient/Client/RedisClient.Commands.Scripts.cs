using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Models;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private ScriptCommands _scriptCommands;
        public ScriptCommands Scripts => _scriptCommands;
        public class ScriptCommands
        {
            private readonly RedisClient _redisClient;

            internal LuaScript MultiExpireScript;
            internal LuaScript MultiSetexScript;

            internal ScriptCommands(RedisClient redisClient)
            {
                _redisClient = redisClient;
            }

            public async Task FlushScripts(CancellationToken cancellationToken)
            {
                await _redisClient.RunWithTimeout(async token =>
                {
                    var command = RedisEncoder.EncodeCommand(Commands.Script, Commands.Flush);
                    
                    await _redisClient.SendOrQueueAsync(command, _redisClient.SuccessResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false);
            }

            public async Task<LuaScript> LoadScript(string script, CancellationToken cancellationToken)
            {
                var command = RedisEncoder.EncodeCommand(Commands.Script, Commands.Load, script.AsReadOnlyByteMemory());
                var scriptResponse = await _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendOrQueueAsync(command, _redisClient.DataResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false);
                
                return new LuaScript(_redisClient, scriptResponse);
            }

            internal async Task<RawResult> EvalSha(string sha1Hash, IEnumerable<string> keys, IEnumerable<string> arguments, CancellationToken cancellationToken)
            {
                var keysList = keys.ToList();
                var command = RedisEncoder.EncodeCommand(Commands.EvalSha, sha1Hash.AsReadOnlyByteMemory(),
                    keysList.ToString().AsReadOnlyByteMemory(), arguments.ToString().AsReadOnlyByteMemory());

                var scriptResult = await _redisClient.RunWithTimeout(async token =>
                    {
                        return await _redisClient.SendOrQueueAsync(command, _redisClient.GenericResultProcessor, token);
                    },
                    cancellationToken).ConfigureAwait(false);
                
                return scriptResult;
            }
        }
    }
}