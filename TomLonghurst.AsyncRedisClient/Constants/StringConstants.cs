namespace TomLonghurst.AsyncRedisClient.Constants
{
    public static class StringConstants
    {
        public static string EncodedLineTerminator { get; } = @"\r\n";
        public static string EncodedNewLine { get; } = @"\n";
        
        public static string LineTerminator { get; } = "\r\n";
        public static string NewLine { get; } = "\n";
    }
}