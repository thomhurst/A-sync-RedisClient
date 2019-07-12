using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Helpers;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Extensions
{
    internal static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] ToUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

#if NETCORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ToUtf8Bytes(this ReadOnlySpan<char> value, out Span<byte> buffer)
        {
            buffer = new byte[value.Length].AsSpan();
            Encoding.UTF8.GetBytes(value, buffer);
        }
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToRedisProtocol(this string value)
        {
            return RedisProtocolEncoder.Encode(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromRedisProtocol(this string value)
        {
            return RedisProtocolEncoder.Decode(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<string> Split(this string value, string delimiter)
        {
            return value.Split(new[] {delimiter}, StringSplitOptions.None);
        }

        internal static IEnumerable<RedisValue<string>> ToRedisValues(this string[] values)
        {
            return values.Select(value => new RedisValue<string>(value));
        }
    }
}
