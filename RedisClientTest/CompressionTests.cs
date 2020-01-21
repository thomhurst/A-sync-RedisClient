using System.Threading.Tasks;
using NUnit.Framework;
using TomLonghurst.AsyncRedisClient.Compression;

namespace RedisClientTest
{
    public class CompressionTests : TestBase
    {
        [Test]
        public async Task NoCompression()
        {
            await TestUtilities.TimeAsync(async () =>
            {
                await (await TomLonghurstRedisClient).StringSetAsync("NoCompression",
                    _largeValueJson,
                    CompressionType.None);
                
                var result =
                    await (await TomLonghurstRedisClient).StringGetAsync("NoCompression", CompressionType.None);

                Assert.That(result.Value, Is.EqualTo(_largeValueJson));

                await (await TomLonghurstRedisClient).ExpireAsync("NoCompression", 30);
            });
        }

        [Test]
        public async Task BrotliCompression()
        {
            await TestUtilities.TimeAsync(async () =>
            {
                await (await TomLonghurstRedisClient).StringSetAsync("Brotli",
                    _largeValueJson,
                    CompressionType.Brotli);
                var result = await (await TomLonghurstRedisClient).StringGetAsync("Brotli", CompressionType.Brotli);

                Assert.That(result.Value, Is.EqualTo(_largeValueJson));

                await (await TomLonghurstRedisClient).ExpireAsync("Brotli", 30);
            });
        }

        [Test]
        public async Task GzipCompression()
        {
            await TestUtilities.TimeAsync(async () =>
            {
                await (await TomLonghurstRedisClient).StringSetAsync("Gzip",
                    _largeValueJson,
                    CompressionType.GZip);
                var result = await (await TomLonghurstRedisClient).StringGetAsync("Gzip", CompressionType.GZip);

                Assert.That(result.Value, Is.EqualTo(_largeValueJson));

                await (await TomLonghurstRedisClient).ExpireAsync("Gzip", 30);
            });
        }

        [Test]
        public async Task LZ4Compression()
        {
            await TestUtilities.TimeAsync(async () =>
            {
                await (await TomLonghurstRedisClient).StringSetAsync("LZ4",
                    _largeValueJson,
                    CompressionType.LZ4);
                var result = await (await TomLonghurstRedisClient).StringGetAsync("LZ4", CompressionType.LZ4);

                Assert.That(result.Value, Is.EqualTo(_largeValueJson));

                await (await TomLonghurstRedisClient).ExpireAsync("LZ4", 30);
            });
        }
    }
}