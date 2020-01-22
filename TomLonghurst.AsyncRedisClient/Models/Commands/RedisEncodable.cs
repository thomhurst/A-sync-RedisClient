using System.Collections.Generic;
using System.Linq;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public struct RedisEncodable : IRedisEncodable
    {
        private string[] StringCommands { get; }
        
        public string AsString => string.Join(" ", StringCommands);
        
        private RedisEncodable(params string[] stringCommands)
        {
            StringCommands = stringCommands;
        }

        public IEnumerable<byte> GetEncodedCommand()
        {
            var _encodedCommand = new List<byte[]> {$"*{StringCommands.Length}".ToUtf8BytesWithTerminator()};

            foreach (var stringCommands in StringCommands)
            {
                var bytes = stringCommands.ToUtf8BytesWithTerminator();
                _encodedCommand.Add($"${bytes.Length - 2}".ToUtf8BytesWithTerminator());
                _encodedCommand.Add(bytes);
            }

            return _encodedCommand.SelectMany(ec => ec);
        }

        public static RedisEncodable From(string stringCommand)
        {
            return new RedisEncodable(stringCommand);
        }

        public static RedisEncodable FromScript(string commandPrefix, string sha1Hash, List<string> keysList, IEnumerable<string> arguments)
        {
            return From($"{commandPrefix} {sha1Hash} {keysList.Count} {string.Join(" ", keysList)} {string.Join(" ", arguments)}");
        }
    }
}