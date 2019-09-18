using System.Collections.Generic;
using System.Linq;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Sample<T>(this IEnumerable<T> source, int interval)
        {
            return source?.Where((value, index) => (index + 1) % interval == 0);
        }
    }
}