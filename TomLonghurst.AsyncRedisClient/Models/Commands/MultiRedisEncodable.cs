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
        
        private MultiRedisEncodable(IEnumerable<IRedisEncodable> commands)
        {
            _stringBuilder = new StringBuilder();
            foreach (var stringCommand in commands)
            {
                _stringBuilder.Append(stringCommand);
                _stringBuilder.Append("\r\n");
            }
        }

        public IEnumerable<byte> GetEncodedCommand()
        {
            return AsString.ToUtf8BytesWithTerminator();
        }

        public static MultiRedisEncodable From(IEnumerable<IRedisEncodable> stringCommands)
        {
            return new MultiRedisEncodable(stringCommands);
        }
    }
}