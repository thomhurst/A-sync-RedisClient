using System.Collections.Generic;
using System.Linq;

namespace TomLonghurst.RedisClient.Models.Commands
{
    public class MultiRedisCommand : IRedisCommand
    {
        public IList<byte[]> EncodedCommandList { get; }
        public string AsString { get; }

        public MultiRedisCommand(IEnumerable<IRedisCommand> redisCommands)
        {
            EncodedCommandList = redisCommands.SelectMany(x => x.EncodedCommandList).ToArray();
        }
        
        public static MultiRedisCommand From(IEnumerable<IRedisCommand> redisCommands)
        {
            return new MultiRedisCommand(redisCommands);
        }
    }
}