using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Models.Commands;

namespace TomLonghurst.AsyncRedisClient.Extensions;

internal static class StringExtensions
{
    internal static unsafe byte[] ToUtf8Bytes(this string value)
    {
        var encodedLength = Encoding.UTF8.GetByteCount(value);
        var byteArray = new byte[encodedLength];

        fixed (char* charPtr = value)
        {
            fixed (byte* bytePtr = byteArray)
            {
                Encoding.UTF8.GetBytes(charPtr, value.Length, bytePtr, encodedLength);
            }
        }

        return byteArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal static unsafe byte[] ToUtf8BytesWithTerminator(this ReadOnlySpan<char> value)
    {
        var encodedLength = Encoding.UTF8.GetByteCount(value);
        var byteArray = new byte[encodedLength + 2];

        fixed (char* charPtr = value)
        {
            fixed (byte* bytePtr = byteArray)
            {
                Encoding.UTF8.GetBytes(charPtr, value.Length, bytePtr, encodedLength);
            }
        }

        byteArray[encodedLength] = ByteConstants.BackslashR;
        byteArray[encodedLength + 1] = ByteConstants.NewLine;

        return byteArray;
    }
        
        
    internal static unsafe int AsUtf8BytesSpan(this string value, out Span<byte> bytesSpan)
    {
        var charsSpan = value.AsSpan();

        fixed(char* charPtr = charsSpan)
        {
            bytesSpan = new byte[Encoding.UTF8.GetByteCount(charPtr, charsSpan.Length)].AsSpan();
            fixed (byte* bytePtr = bytesSpan)
            {
                return Encoding.UTF8.GetBytes(charPtr, charsSpan.Length, bytePtr, bytesSpan.Length);
            }
        }
    }
        
        
    internal static unsafe int AsUtf8BytesSpanWithTerminator(this string value, out Span<byte> bytesSpan)
    {
        var charsSpan = value.AsSpan();
        int encodedLength;
                
        fixed(char* charPtr = charsSpan)
        {
            encodedLength = Encoding.UTF8.GetByteCount(charPtr, charsSpan.Length);
            bytesSpan = new byte[encodedLength + 2].AsSpan();
            fixed (byte* bytePtr = bytesSpan)
            {
                Encoding.UTF8.GetBytes(charPtr, charsSpan.Length, bytePtr, bytesSpan.Length);
            }
        }
            
        bytesSpan[encodedLength] = ByteConstants.BackslashR;
        bytesSpan[encodedLength + 1] = ByteConstants.NewLine;

        return encodedLength + 2;
    }
        
        
    internal static IEnumerable<string> Split(this string value, string delimiter)
    {
        return value.Split([delimiter], StringSplitOptions.RemoveEmptyEntries);
    }
        
    // internal static IRedisCommand ToPipelinedCommand(this IEnumerable<IRedisCommand> commands)
    // {
    //     var enumerable = commands.ToList();
    //     
    //     if (enumerable.Count > 1)
    //     {
    //         return MultiRedisCommand.From(enumerable);
    //     }
    //
    //     return enumerable[0];
    // }
}