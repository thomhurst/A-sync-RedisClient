using System.Collections.ObjectModel;
using System.Linq;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Constants
{
    public static class ByteConstants
    {
        public static readonly byte[] LINE_TERMINATOR = new ReadOnlyCollection<byte>("\r\n".ToUtf8Bytes().ToList()).ToArray();
        public static readonly byte[] NEW_LINE = new ReadOnlyCollection<byte>("\n".ToUtf8Bytes().ToList()).ToArray();

        public const byte BackslashR = (byte) '\r';
        public const byte NewLine = (byte) '\n';
        
        public const byte Plus = (byte) '+';
        public const byte Dash = (byte) '-';
        public const byte Dollar = (byte) '$';
        public const byte Asterix = (byte) '*';
        public const byte Colon = (byte) ':';
        
        public const byte One = (byte) '1';
        
        public const byte O = (byte) 'O';
        public const byte K = (byte) 'K';
    }
}