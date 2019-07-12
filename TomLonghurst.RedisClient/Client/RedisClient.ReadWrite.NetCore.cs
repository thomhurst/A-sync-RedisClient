using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
#if NETCORE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<Memory<byte>> ReadData()
        {
            var line = ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var firstChar = line.First();

            if (firstChar == '-')
            {
                throw new RedisFailedCommandException(line);
            }

            if (firstChar == '$')
            {
                if (line == "$-1")
                {
                    return null;
                }

                if (int.TryParse(line.Substring(1), out var byteSizeOfData))
                {
                    var byteBuffer = new byte [byteSizeOfData].AsMemory();

                    var bytesRead = 0;
                    do
                    {
                        var read = await _bufferedStream.ReadAsync(byteBuffer);

                        if (read < 1)
                        {
                            throw new UnexpectedRedisResponseException(
                                $"Invalid termination mid stream: {byteBuffer.FromUtf8()}");
                        }

                        bytesRead += read;
                    } while (bytesRead < byteSizeOfData);

                    if (_bufferedStream.ReadByte() != '\r' || _bufferedStream.ReadByte() != '\n')
                    {
                        throw new UnexpectedRedisResponseException($"Invalid termination: {byteBuffer.FromUtf8()}");
                    }

                    return byteBuffer;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }
#endif
    }
}