using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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
        internal static IEnumerable<string> FromUtf8(this IEnumerable<byte[]> bytesArray)
        {
            return bytesArray.Select(bytes => bytes.FromUtf8()).Where(s => s != null);
        }
    }
}