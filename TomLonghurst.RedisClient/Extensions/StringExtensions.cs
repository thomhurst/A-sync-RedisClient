using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Constants;
using TomLonghurst.RedisClient.Models.Commands;

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
        internal static byte[] ToUtf8BytesWithTerminator(this string value)
        {
            return value.ToUtf8Bytes().Concat(ByteConstants.LINE_TERMINATOR).ToArray();
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
        internal static IEnumerable<string> Split(this string value, string delimiter)
        {
            return value.Split(new[] {delimiter}, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static IRedisCommand ToFireAndForgetCommand(this IEnumerable<RedisCommand> commands)
        {
            var enumerable = commands.ToList();
            if (enumerable.Count > 1)
            {
                var clientReplyOff = RedisCommand.From("CLIENT".ToRedisEncoded(),
                    "REPLY".ToRedisEncoded(),
                    "OFF".ToRedisEncoded());

                var fireAndForgetCommands = new List<RedisCommand> { clientReplyOff };
                
                fireAndForgetCommands.AddRange(enumerable);
                
                var clientReplyOn = RedisCommand.From("CLIENT".ToRedisEncoded(),
                    "REPLY".ToRedisEncoded(),
                    "ON".ToRedisEncoded());
            
                fireAndForgetCommands.Add(clientReplyOn);
                
                return MultiRedisCommand.From(fireAndForgetCommands);
            }
            else
            {
                return enumerable.First();
            }
        }
    }
}
