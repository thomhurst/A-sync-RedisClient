using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Constants
{
    public static class ByteConstants
    {
        public static readonly byte[] LINE_TERMINATOR = "\r\n".ToUtf8Bytes();
        public static readonly byte[] NEW_LINE = "\n".ToUtf8Bytes();
    }
}