using System.Collections.Generic;
using System.Linq;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Models.Commands
{
    public class RedisCommand : IRedisCommand
    {
        private readonly IEnumerable<IRedisEncodable> _redisEncodables;
        private readonly byte[][] redisCommand;
        private byte[] _preEncodedCommand;

        public byte[] EncodedCommand
        {
            get
            {
                if (_preEncodedCommand != null)
                {
                    return _preEncodedCommand;
                }
                
                return $"*{redisCommand.Length}".ToUtf8BytesWithTerminator()
                    .Concat(redisCommand.SelectMany(x => $"${x.Length - 2}".ToRedisEncoded().RedisEncodedBytes.Concat(x)))
                    .ToArray();
            }
        }

        public string AsString => string.Join(" ", _redisEncodables.Select(x => x.AsString));

        private RedisCommand(IEnumerable<IRedisEncodable> redisEncodables)
        {
            _redisEncodables = redisEncodables;
            redisCommand = redisEncodables.Select(x => x.RedisEncodedBytes).ToArray();
        }
        
        public static RedisCommand From(IEnumerable<IRedisEncodable> redisEncodables)
        {
            return new RedisCommand(redisEncodables);
        }
        
        private RedisCommand(params IRedisEncodable[] redisEncodables)
        {
            _redisEncodables = redisEncodables;
            redisCommand = redisEncodables.Select(x => x.RedisEncodedBytes).ToArray();
        }
        
        public static RedisCommand From(params IRedisEncodable[] redisEncodables)
        {
            return new RedisCommand(redisEncodables);
        }
        
//        public static RedisCommand From(IRedisEncodable redisEncodable, params IRedisEncodable[] redisEncodables)
//        {
//            return new RedisCommand(new List<IRedisEncodable> { redisEncodable }.Concat(redisEncodables));
//        }

        private RedisCommand(IRedisEncodable redisEncodable)
        {
            _redisEncodables = new[] {redisEncodable};
            redisCommand = new []{ redisEncodable.RedisEncodedBytes };
        }

        private RedisCommand(IEnumerable<IRedisCommand> redisEncodables) : this(redisEncodables.Select(x => x.EncodedCommand))
        {
        }

        private RedisCommand(IEnumerable<byte[]> bytes)
        {
            var joinedArray = new List<byte>();
            
            foreach (var bytese in bytes)
            {
                joinedArray.AddRange(bytese);
            }

            _preEncodedCommand = joinedArray.ToArray();
        }

        public static RedisCommand From(IRedisEncodable redisEncodable)
        {
            return new RedisCommand(redisEncodable);
        }
        
        public static RedisCommand From(IEnumerable<IRedisCommand> redisCommands)
        {
            return new RedisCommand(redisCommands);
        }

        public static IRedisCommand From(IRedisEncodable redisEncodable, IEnumerable<IRedisEncodable> redisEncodables)
        {
            return new RedisCommand(new List<IRedisEncodable> { redisEncodable }.Concat(redisEncodables));
        }
    }
}