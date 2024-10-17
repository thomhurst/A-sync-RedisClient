using System.Buffers;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Helpers;

internal static class SpanNumberParser
{
    internal static int Parse(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        if (buffer.Length >= 2 && buffer.ItemAt(0) == ByteConstants.Dash && buffer.ItemAt(1) == ByteConstants.One)
        {
            return -1;
        }
            
        if (!char.IsDigit((char) buffer.ItemAt(0)) && buffer.ItemAt(0) != ByteConstants.Dash)
        {
            return Parse(buffer.Slice(buffer.GetPosition(1, buffer.Start)));
        }

        if (buffer.GetEndOfLinePosition() != null)
        {
            return Parse(buffer.Slice(buffer.Start, buffer.Length - 2));
        }

        return ParseSequence(buffer);
    }
        
    internal static long Parse(params byte[] byteValues)
    {
        return (long) byteValues.Select((t, i) => GetValue(t) * Math.Pow(10, (double) byteValues.Length - i - 1)).Sum();
    }
        
    internal static int ParseSequence(ReadOnlySequence<byte> byteValues)
    {
        var result = 0;
        var outerIndex = 0;
        foreach (var readOnlyMemory in byteValues)
        {
            foreach (var b in readOnlyMemory.Span)
            {
                result += (char) GetValue(b) * (int) Math.Pow(10, byteValues.Length - 1 - outerIndex);
                outerIndex++;
            }
        }

        return result;
    }
        
    internal static long GetValue(byte byteValue)
    {
        return (long) char.GetNumericValue((char) byteValue);
    } 
}