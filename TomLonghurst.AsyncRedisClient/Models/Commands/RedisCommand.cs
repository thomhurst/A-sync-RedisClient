using System.Collections.Generic;
using System.Linq;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public class RedisCommand : IRedisCommand
    {
        internal readonly IEnumerable<IRedisEncodable> _redisEncodables;
        internal readonly byte[][] rawBytes;

        private IList<byte[]> _encodedCommand;

        public IList<byte[]> EncodedCommandList
        {
            get
            {
                if (_encodedCommand != null)
                {
                    return _encodedCommand;
                }

                _encodedCommand = new List<byte[]> {$"*{rawBytes.Length}".ToUtf8BytesWithTerminator()};

                foreach (var rawByte in rawBytes)
                {
                    _encodedCommand.Add($"${rawByte.Length - 2}".ToUtf8BytesWithTerminator());
                    _encodedCommand.Add(rawByte);
                }

                return _encodedCommand;
            }
        }

        public string AsString => string.Join(" ", _redisEncodables.Select(x => x.AsString));

        private RedisCommand(IEnumerable<IRedisEncodable> redisEncodables)
        {
            _redisEncodables = redisEncodables;
            rawBytes = redisEncodables.Select(x => x.RedisEncodedBytes).ToArray();
        }
        
        public static RedisCommand From(IEnumerable<IRedisEncodable> redisEncodables)
        {
            return new RedisCommand(redisEncodables);
        }
        
        private RedisCommand(params IRedisEncodable[] redisEncodables)
        {
            _redisEncodables = redisEncodables;
            rawBytes = redisEncodables.Select(x => x.RedisEncodedBytes).ToArray();
        }
        
        public static RedisCommand From(params IRedisEncodable[] redisEncodables)
        {
            return new RedisCommand(redisEncodables);
        }

        private RedisCommand(IRedisEncodable redisEncodable)
        {
            _redisEncodables = new[] {redisEncodable};
            rawBytes = new []{ redisEncodable.RedisEncodedBytes };
        }

        public static RedisCommand From(IRedisEncodable redisEncodable)
        {
            return new RedisCommand(redisEncodable);
        }

        public static IRedisCommand From(IRedisEncodable redisEncodable, IEnumerable<IRedisEncodable> redisEncodables)
        {
            return new RedisCommand(new List<IRedisEncodable> { redisEncodable }.Concat(redisEncodables));
        }
    }
}