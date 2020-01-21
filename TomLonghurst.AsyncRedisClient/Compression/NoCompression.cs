using System.Threading.Tasks;

namespace TomLonghurst.AsyncRedisClient.Compression
{
    public class NoCompression : ICompression
    {
        public ValueTask<string> DecompressAsync(string compressedData)
        {
            return new ValueTask<string>(compressedData);
        }

        public ValueTask<string> CompressAsync(string uncompressedData)
        {
            return new ValueTask<string>(uncompressedData);
        }
    }
}