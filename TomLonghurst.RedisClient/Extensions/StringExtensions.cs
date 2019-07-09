using System;
using System.Collections.Generic;
using System.Text;
using TomLonghurst.RedisClient.Helpers;

namespace TomLonghurst.RedisClient.Extensions
{
    internal static class StringExtensions
    {
        internal static byte[] ToUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
        
        internal static string ToRedisProtocol(this string value)
        {
            return RedisProtocolEncoder.Encode(value);
        }
        
        internal static string FromRedisProtocol(this string value)
        {
            return RedisProtocolEncoder.Decode(value);
        }

        internal static IEnumerable<string> Split(this string value, string delimiter)
        {
            return value.Split(new[] {delimiter}, StringSplitOptions.None);
        }
    }
}
