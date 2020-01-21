namespace TomLonghurst.AsyncRedisClient.Compression
{
    public static class CompressionType
    {
        public static NoCompression None { get; } = new NoCompression();
        public static BrotliCompression Brotli { get; } = new BrotliCompression();
        public static GZipCompression GZip { get; } = new GZipCompression();
        public static LZ4Compression LZ4 { get; } = new LZ4Compression();
        public static ICompression Custom(ICompression compression) => compression;
    }
}