using System.Collections.ObjectModel;
using System.Linq;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Constants
{
    public static class ByteConstants
    {
        public static readonly byte[] LINE_TERMINATOR = new ReadOnlyCollection<byte>("\r\n".ToUtf8Bytes().ToList()).ToArray();
        public static readonly byte[] NEW_LINE = new ReadOnlyCollection<byte>("\n".ToUtf8Bytes().ToList()).ToArray();
    }
}