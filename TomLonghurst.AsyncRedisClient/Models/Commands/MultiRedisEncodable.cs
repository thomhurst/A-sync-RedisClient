using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public struct MultiRedisEncodable : IRedisEncodable
    {
        private StringBuilder _stringBuilder;
        public string AsString => _stringBuilder.ToString();
        
        private MultiRedisEncodable(IEnumerable<string> stringCommands)
        {
            _stringBuilder = new StringBuilder();
            foreach (var stringCommand in stringCommands)
            {
                _stringBuilder.Append(stringCommand);
                _stringBuilder.Append("\r\n");
            }
        }

        public byte[] GetEncodedCommand()
        {
            return AsString.ToUtf8BytesWithTerminator();
        }

        public static MultiRedisEncodable From(IEnumerable<string> stringCommands)
        {
            return new MultiRedisEncodable(stringCommands);
        }
        
        public static MultiRedisEncodable From(IEnumerable<IRedisEncodable> commands)
        {
            return From(commands.Select(c => c.AsString));
        }
    }
}