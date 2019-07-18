using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Helpers
{
    internal static class RedisProtocolEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Encode(string command)
        {
            if (command.StartsWith("*"))
            {
                // Already Encoded!
                return command;
            }

            if (command.Contains("\r\n"))
            {
                var multipleCommands = command.Split("\r\n").ToList();

                if (multipleCommands.Any())
                {
                    return EncodeMultiple(multipleCommands);
                }
            }

            string[] commands;
            if (command.Contains('"'))
            {
                var firstQuoteIndex = command.IndexOf('"');
                var lastQuoteIndex = command.LastIndexOf('"');
                
                var quotedText = command.Substring(firstQuoteIndex, lastQuoteIndex - firstQuoteIndex + 1);

                var commandWithoutQuotedText = command.Replace(quotedText, string.Empty);

                quotedText = quotedText.Substring(1, quotedText.Length - 2);
                
                var commandsList = commandWithoutQuotedText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                commandsList.Add(quotedText);
                commands = commandsList.ToArray();
            }
            else
            {
                commands = command.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }

            var sb = new StringBuilder($"*{commands.Length}\r\n");

            foreach (var c in commands)
            {
                
                
                sb.Append($"${c.ToUtf8Bytes().Length}\r\n");
                sb.Append($"{c}\r\n");
            }

            return sb.ToString();
        }

        private static string EncodeMultiple(IEnumerable<string> multipleCommands)
        {
            var encodingBuilder = new StringBuilder();

            foreach (var multipleCommand in multipleCommands)
            {
                encodingBuilder.Append(Encode(multipleCommand));
            }

            return encodingBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Decode(string response)
        {
            var lines = response.Split("\r\n").ToList();
            lines.RemoveAll(RemoveFilter);

            return lines.Any() ? string.Join(" ", lines) : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool RemoveFilter(string line)
        {
            return string.IsNullOrWhiteSpace(line.Replace('\0', ' ')) || line.StartsWith("$") || line.StartsWith("*");
        }
    }
}