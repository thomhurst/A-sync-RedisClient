using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Models;
#if NETCORE
using System.Buffers;
#endif

namespace TomLonghurst.RedisClient.Extensions
{
    internal static class ByteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromUtf8(this byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<RedisValue<string>> ToRedisValues(this IEnumerable<byte[]> bytesArray)
        {
            return bytesArray.Select(bytes => new RedisValue<string>(bytes.FromUtf8()));
        }

#if NETCORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromUtf8(this ReadOnlySequence<byte> buffer)
        {
            return string.Create((int) buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.UTF8.GetChars(segment.Span, span);

                    span = span.Slice(segment.Length);
                }
            });
        }
#endif

#if NETCORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string FromUtf8(this Span<byte> buffer)
        {
            var stringBuilder = new StringBuilder();

            if (buffer.Length > 0)
            {
                fixed (byte* bytes = buffer)
                {
                    stringBuilder.Append(Encoding.UTF8.GetString(bytes, buffer.Length));
                }
            }
            else
            {
                return null;
            }

            return stringBuilder.ToString();
        }
#endif

#if NETCORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromUtf8(this Memory<byte> buffer)
        {
            return buffer.Span.FromUtf8();
        }
#endif
    }
}