using System.Collections.Generic;
using System.Linq;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Models.Commands
{
    public class RedisCommand : IRedisCommand
    {
        internal readonly IEnumerable<IRedisEncodable> _redisEncodables;
        internal readonly byte[][] rawBytes;

        public byte[] EncodedCommand
        {
            get
            {
                return $"*{rawBytes.Length}".ToUtf8BytesWithTerminator()
                    .Concat(rawBytes.SelectMany(x => $"${x.Length - 2}".ToRedisEncoded().RedisEncodedBytes.Concat(x)))
                    .ToArray();
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
        
//        public static RedisCommand From(IRedisEncodable redisEncodable, params IRedisEncodable[] redisEncodables)
//        {
//            return new RedisCommand(new List<IRedisEncodable> { redisEncodable }.Concat(redisEncodables));
//        }

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