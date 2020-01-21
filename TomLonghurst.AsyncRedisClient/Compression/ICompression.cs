using System.Threading.Tasks;

namespace TomLonghurst.AsyncRedisClient.Compression
{
    public interface ICompression
    {
        ValueTask<string> DecompressAsync(string compressedData);
        ValueTask<string> CompressAsync(string uncompressedData);
    }
}