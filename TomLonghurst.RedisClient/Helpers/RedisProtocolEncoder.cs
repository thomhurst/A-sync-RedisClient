using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.RedisClient.Constants;
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
            if (command.Contains(CharacterConstants.VALUE_DELIMITER))
            {
                var commandsSeparated = command.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var commandsList = new List<string>();

                for (var index = 0; index < commandsSeparated.Count; index++)
                {
                    var cmd = commandsSeparated[index];
                    if (cmd != CharacterConstants.VALUE_DELIMITER)
                    {
                        commandsList.Add(cmd);
                    }
                    else
                    {
                        cmd = commandsSeparated[++index];
                        
                        var quotedTextStringBuilder = new StringBuilder();
                        quotedTextStringBuilder.Append(cmd);
                        
                        cmd = commandsSeparated[++index];
                        while (cmd != CharacterConstants.VALUE_DELIMITER)
                        {
                            quotedTextStringBuilder.Append(" ");
                            quotedTextStringBuilder.Append(cmd);
                            cmd = commandsSeparated[++index];    
                        }

                        commandsList.Add(quotedTextStringBuilder.ToString());
                    }
                }
                
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