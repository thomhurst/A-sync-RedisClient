using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomLonghurst.AsyncRedisClient.Models;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    internal static class ByteExtensions
    {
        
        internal static string FromUtf8(this byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }
        
        
        internal static IEnumerable<string> ToStringsFromUtf8(this IEnumerable<byte[]> bytesArray)
        {
            return bytesArray.Select(bytes => bytes.FromUtf8());
        }
    }
}