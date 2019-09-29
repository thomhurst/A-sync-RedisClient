using System.Collections.ObjectModel;
using System.Linq;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Constants
{
    public static class ByteConstants
    {
        public static byte BackslashR { get; } = (byte) '\r';
        public static byte NewLine { get; } = (byte) '\n';
        
        public static byte Plus { get; } = (byte) '+';
        public static byte Dash { get; } = (byte) '-';
        public static byte Dollar { get; } = (byte) '$';
        public static byte Asterix { get; } = (byte) '*';
        public static byte Colon { get; } = (byte) ':';
        
        public static byte One { get; } = (byte) '1';
        
        public static byte O { get; } = (byte) 'O';
        public static byte K { get; } = (byte) 'K';
    }
}