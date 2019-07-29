using System.Collections.Generic;
using System.Linq;

namespace TomLonghurst.RedisClient.Models.Commands
{
    public class MultiRedisCommand : IRedisCommand
    {
        public byte[] EncodedCommand { get; }
        public string AsString { get; }

        public MultiRedisCommand(IEnumerable<IRedisCommand> redisCommands)
        {
            EncodedCommand = redisCommands.SelectMany(x => x.EncodedCommand).ToArray();
        }
        
        public static MultiRedisCommand From(IEnumerable<IRedisCommand> redisCommands)
        {
            return new MultiRedisCommand(redisCommands);
        }
    }
}