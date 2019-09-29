using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private ScriptCommands _scriptCommands;
        public ScriptCommands Scripts => _scriptCommands;
        public class ScriptCommands
        {
            private readonly RedisClient _redisClient;

            internal const string MultiSetExpire = "MultiSetExpire";
            
            internal readonly Dictionary<string, string> ScriptNameToShaKey = new Dictionary<string, string>();

            internal Lazy<Task<string>> LazyMultiExpireLuaScript;
            
            internal ScriptCommands(RedisClient redisClient)
            {
                _redisClient = redisClient;
                CreateLazyScripts();
            }

            private void CreateLazyScripts()
            {
                LazyMultiExpireLuaScript = new Lazy<Task<string>>(async () => await LoadScript(MultiSetExpire,
                    $"for i, name in ipairs(KEYS) do redis.call(\"EXPIRE\", name, ARGV[1]); end",
                    CancellationToken.None));
            }

            public async Task FlushScripts(CancellationToken cancellationToken)
            {
                await _redisClient.RunWithTimeout(async token =>
                {
                    await _redisClient.SendOrQueueAsync(Commands.ScriptFlush, _redisClient.SuccessResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false);
            }

            public async Task<string> LoadScript(string script, CancellationToken cancellationToken)
            {
                var command = RedisCommand.From(Commands.Script, Commands.Load, script.ToRedisEncoded());
                var scriptResponse = await _redisClient.RunWithTimeout(async token =>
                {
                    return await _redisClient.SendOrQueueAsync(command, _redisClient.DataResultProcessor, token);
                }, cancellationToken).ConfigureAwait(false);
                
                return scriptResponse;
            }

            private async Task<string> LoadScript(string scriptName, string script, CancellationToken cancellationToken)
            {
                var sha1 = await LoadScript(script, cancellationToken).ConfigureAwait(false);
                ScriptNameToShaKey[scriptName] = sha1;
                return sha1;
            }

            public async Task<RawResult> EvalSha(string sha1Hash, IEnumerable<string> keys, IEnumerable<string> arguments, CancellationToken cancellationToken)
            {
                var scriptHash = ScriptNameToShaKey.ContainsKey(sha1Hash) ? ScriptNameToShaKey[sha1Hash] : sha1Hash;
                
                var keysList = keys.ToList();
                var command = RedisCommand.FromScript(Commands.EvalSha, scriptHash.ToRedisEncoded(), keysList, arguments);

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