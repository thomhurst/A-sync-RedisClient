using System.Collections.Generic;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public struct RedisEncodable : IRedisEncodable
    {
        public string AsString { get; }
        
        private RedisEncodable(string stringCommand)
        {
            AsString = stringCommand;
        }

        public byte[] GetEncodedCommand()
        {
            return AsString.ToUtf8BytesWithTerminator();
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