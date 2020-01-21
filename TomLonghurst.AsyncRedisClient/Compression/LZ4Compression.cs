using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using K4os.Compression.LZ4.Streams;

namespace TomLonghurst.AsyncRedisClient.Compression
{
    public class LZ4Compression : ICompression
    {
        public async ValueTask<string> DecompressAsync(string compressedData)
        {
            if (string.IsNullOrEmpty(compressedData))
            {
                return null;
            }

            var bytes = Convert.FromBase64String(compressedData);

#if NETCOREAPP3_1
            await using var memoryStreamIn = new MemoryStream(bytes);
            await using var memoryStreamOut = new MemoryStream();

            await using (var compressionStream = LZ4Stream.Decode(memoryStreamIn))
            {
                await compressionStream.CopyToAsync(memoryStreamOut);
            }

#else
            using var memoryStreamIn = new MemoryStream(bytes);
            using var memoryStreamOut = new MemoryStream();
            using (var compressionStream = LZ4Stream.Decode(memoryStreamIn))
            {
                await compressionStream.CopyToAsync(memoryStreamOut);
            }
#endif

            return Encoding.UTF8.GetString(memoryStreamOut.ToArray());
        }

        public async ValueTask<string> CompressAsync(string uncompressedData)
        {
            if (string.IsNullOrEmpty(uncompressedData))
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(uncompressedData);

#if NETCOREAPP3_1
            await using var memoryStreamIn = new MemoryStream(bytes);
            await using var memoryStreamOut = new MemoryStream();
            await using (var compressionStream = LZ4Stream.Encode(memoryStreamOut))
            {
                await memoryStreamIn.CopyToAsync(compressionStream);
            }
#else
            using var memoryStreamIn = new MemoryStream(bytes);
            using var memoryStreamOut = new MemoryStream();
            using (var compressionStream = LZ4Stream.Encode(memoryStreamOut))
            {
                await memoryStreamIn.CopyToAsync(compressionStream);
            }
#endif

            return Convert.ToBase64String(memoryStreamOut.ToArray());
        }
    }
}