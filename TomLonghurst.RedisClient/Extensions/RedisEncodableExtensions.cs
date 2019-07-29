using System.Collections.Generic;
using System.Linq;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class RedisEncodableExtensions
    {
        internal static IRedisEncodable ToRedisEncoded(this string value)
        {
            return new RedisEncodable(value);
        }
        
        internal static IEnumerable<IRedisEncodable> ToRedisEncoded(this IEnumerable<string> values)
        {
            return values.Select(x => x.ToRedisEncoded());
        }
        
        internal static IRedisEncodable ToRedisEncoded(this int value)
        {
            return new RedisEncodable(value.ToString());
        }
        
        internal static IRedisEncodable ToRedisEncoded(this float value)
        {
            return new RedisEncodable(value.ToString());
        }
        
        internal static IRedisEncodable ToRedisEncoded(this long value)
        {
            return new RedisEncodable(value.ToString());
        }
        
        internal static IRedisCommand ToRedisCommand(this string value)
        {
            return RedisCommand.From(new RedisEncodable(value));
        }
    }
}