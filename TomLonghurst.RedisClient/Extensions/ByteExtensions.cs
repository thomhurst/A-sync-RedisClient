using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Extensions
{
    internal static class ByteExtensions
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromUtf8(this byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }
        
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<StringRedisValue> ToRedisValues(this IEnumerable<byte[]> bytesArray)
        {
            return bytesArray.Select(bytes => new StringRedisValue(bytes.FromUtf8()));
        }
    }
}