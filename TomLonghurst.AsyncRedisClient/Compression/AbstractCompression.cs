using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace TomLonghurst.AsyncRedisClient.Compression
{
    public abstract class AbstractCompression<TCompressionEncoder, TCompressionDecoder> : ICompression 
        where TCompressionEncoder : Stream
        where TCompressionDecoder : Stream
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
            await using (var compressionStream =
                (TCompressionDecoder) Activator.CreateInstance(typeof(TCompressionDecoder),
                    memoryStreamIn,
                    CompressionMode.Decompress))
            {
                await compressionStream.CopyToAsync(memoryStreamOut);
            }

            return Encoding.UTF8.GetString(memoryStreamOut.ToArray());
#else
            using var memoryStreamIn = new MemoryStream(bytes);
            using var memoryStreamOut = new MemoryStream();

            using (var compressionStream = (TCompressionDecoder) Activator.CreateInstance(typeof(TCompressionDecoder),
                memoryStreamIn,
                CompressionMode.Decompress))
            {
                await compressionStream.CopyToAsync(memoryStreamOut);
            }

            return Encoding.UTF8.GetString(memoryStreamOut.ToArray());
#endif
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
            await using (var compressionStream =
                (TCompressionEncoder) Activator.CreateInstance(typeof(TCompressionEncoder),
                    memoryStreamOut,
                    CompressionMode.Compress))
            {
                await memoryStreamIn.CopyToAsync(compressionStream);
            }
#else
            using var memoryStreamIn = new MemoryStream(bytes);
            using var memoryStreamOut = new MemoryStream();
            using (var compressionStream = (TCompressionEncoder) Activator.CreateInstance(typeof(TCompressionEncoder),
                memoryStreamOut,
                CompressionMode.Compress))
            {
                await memoryStreamIn.CopyToAsync(compressionStream);
            }
#endif

            return Convert.ToBase64String(memoryStreamOut.ToArray());
        }
    }
}