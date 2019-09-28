using System;

namespace TomLonghurst.AsyncRedisClient.Constants
{
    public ref struct StringConstants
    {
        public const string EncodedLineTerminator = @"\r\n";
        public const string EncodedNewLine = @"\n";
        
        public const string LineTerminator = "\r\n";
        public const string NewLine = "\n";
    }
}