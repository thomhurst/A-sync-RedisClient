using System.Linq;
using System.Text;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Helpers
{
    internal static class RedisProtocolEncoder
    {
        internal static string Encode(string command)
        {
            var commands = command.Split(' ');

            var sb = new StringBuilder($"*{commands.Length}\r\n");

            foreach (var c in commands)
            {
                sb.Append($"${c.ToUtf8Bytes().Length}\r\n");
                sb.Append($"{c}\r\n");
            }

            return sb.ToString();
        }

        internal static string Decode(string response)
        {
            var lines = response.Split("\r\n").ToList();
            lines.RemoveAll(RemoveFilter);

            return lines.Any() ? string.Join(" ", lines) : null;
        }

        private static bool RemoveFilter(string line)
        {
            return string.IsNullOrWhiteSpace(line.Replace('\0', ' ')) || line.StartsWith("$") || line.StartsWith("*");
        }
    }
}