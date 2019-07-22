using System;
using System.Buffers;
using System.Text;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class BufferExtensions
    {
        internal static string AsString(this ReadOnlySequence<byte> buffer)
        {
            // TODO Refactor
            var byteArray = buffer.ToArray();
            return Encoding.UTF8.GetString(byteArray);
        }
        
        internal static int VectorSafeIndexOf(this ReadOnlySpan<byte> span, byte value)
        {
            // yes, this has zero optimization; I'm OK with this as the fallback strategy
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == value) return i;
            }
            return -1;
        }
        internal static int VectorSafeIndexOfCRLF(this ReadOnlySpan<byte> span)
        {
            // yes, this has zero optimization; I'm OK with this as the fallback strategy
            for (int i = 1; i < span.Length; i++)
            {
                if (span[i] == '\n' && span[i-1] == '\r') return i - 1;
            }
            return -1;
        }
    }
}