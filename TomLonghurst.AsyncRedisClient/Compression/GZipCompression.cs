using System.IO.Compression;

namespace TomLonghurst.AsyncRedisClient.Compression
{
    public class GZipCompression : AbstractCompression<GZipStream, GZipStream>
    {
    }
}