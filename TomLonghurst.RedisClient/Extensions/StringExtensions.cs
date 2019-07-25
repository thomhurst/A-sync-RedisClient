using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Helpers;

namespace TomLonghurst.RedisClient.Extensions
{
    internal static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] ToUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int AsUtf8BytesSpan(this string value, out Span<byte> bytesSpan)
        {
            var charsSpan = value.AsSpan();

            fixed(char* charPtr = charsSpan)
            {
                bytesSpan = new byte[Encoding.UTF8.GetByteCount(charPtr, charsSpan.Length)].AsSpan();
                fixed (byte* bytePtr = bytesSpan)
                {
                    return Encoding.UTF8.GetBytes(charPtr, charsSpan.Length, bytePtr, bytesSpan.Length);
                }
            }
        }
        
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
            return value.Split(new[] {delimiter}, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static string ToFireAndForgetCommand(this IEnumerable<string> commands)
        {
            var enumerable = commands.ToList();
            if (enumerable.Count > 1)
            { 
                return $"CLIENT REPLY OFF\r\n{string.Join("\r\n", enumerable)}\r\nCLIENT REPLY ON\r\n";
            }
            else
            {
                return enumerable.First();
            }
        }
    }
}
