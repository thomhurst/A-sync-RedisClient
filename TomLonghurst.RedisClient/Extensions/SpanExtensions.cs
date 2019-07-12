using System;
using System.Text;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class SpanExtensions
    {
        public static int IndexOfLineTerminator(this ReadOnlySpan<byte> byteSpan)
        {
            return byteSpan.IndexOf((byte) '\n') - 1;
        }
        
        internal static unsafe string GetString(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return string.Empty;
            }
            
            fixed (byte* ptr = span)
            {
                return Encoding.UTF8.GetString(ptr, span.Length);
            }
        }
    }
}