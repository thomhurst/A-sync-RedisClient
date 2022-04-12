using System.Text;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient
{
    public class RedisCommandConverter
    {
        private const string LineTerminator = "\r\n";
        private const int LineTerminatorByteLength = 2;
        private const int LineTerminatorCharLength = 2;
        private const int AsterixByteLength = 1;
        private const int DollarByteLength = 1;

        public static byte[] ConvertSingleCommand(string command)
        {
            var words = command.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToArray();

            var byteCount = words.Select(x =>
                            {
                                var wordEncodedLength = Encoding.UTF8.GetByteCount(x);

                                return DollarByteLength + Encoding.UTF8.GetByteCount(wordEncodedLength.ToString()) +
                                       LineTerminatorByteLength
                                       + wordEncodedLength + LineTerminatorByteLength;
                            }).Sum() + AsterixByteLength + Encoding.UTF8.GetByteCount(words.Length.ToString()) +
                            LineTerminatorByteLength;

            var byteArray = new byte[byteCount];
            var bytesCopied = 0;

            var prefix = $"*{words.Length}\r\n";

            Encoding.UTF8.GetBytes(prefix).CopyTo(byteArray, bytesCopied);
            bytesCopied += Encoding.UTF8.GetByteCount(prefix);

            foreach (var word in words)
            {
                var wordEncodedLength = Encoding.UTF8.GetByteCount(word);
                var wordEncodedLengthPrefix = $"${wordEncodedLength}\r\n";

                Encoding.UTF8.GetBytes(wordEncodedLengthPrefix).CopyTo(byteArray, bytesCopied);
                bytesCopied += Encoding.UTF8.GetByteCount(wordEncodedLengthPrefix);

                Encoding.UTF8.GetBytes(word).CopyTo(byteArray, bytesCopied);
                bytesCopied += Encoding.UTF8.GetByteCount(word);

                Encoding.UTF8.GetBytes(LineTerminator).CopyTo(byteArray, bytesCopied);
                bytesCopied += 2;
            }

            return byteArray;
        }

        public static byte[] ConvertMultipleCommands(IReadOnlyCollection<string> commands)
        {
            var bytesList = new List<byte>();
            
            var prefix = $"*{commands.Count}".ToUtf8BytesWithTerminator();
            
            bytesList.AddRange(prefix);

            foreach (var command in commands)
            {
                var rawBytes = command.ToUtf8BytesWithTerminator();
                var encodedCommandSize =$"${rawBytes.Length - 2}".ToUtf8BytesWithTerminator();
                
                bytesList.AddRange(encodedCommandSize);
                bytesList.AddRange(rawBytes);
            }

            return bytesList.ToArray();
        }
    }
}