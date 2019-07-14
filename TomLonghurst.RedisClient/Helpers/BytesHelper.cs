using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class BytesHelper
    {
        public static int GetUtf8BytesAsSpan(string value, out Span<byte> bytes)
        {
            var length = Encoding.UTF8.GetByteCount(value);
            bytes = new byte[length].AsSpan();
            return Encoding.UTF8.GetBytes(value.AsSpan(), bytes);
        }
    }
}