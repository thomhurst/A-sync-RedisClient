using System;
using System.Buffers;
using System.Linq;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Helpers
{
    public static class NumberParser
    {
        public static long Parse(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsEmpty)
            {
                return 0;
            }

            if (buffer.Length >= 2 && buffer.ItemAt(0) == '-' && buffer.ItemAt(1) == '1')
            {
                return -1;
            }
            
            if (!char.IsDigit((char) buffer.ItemAt(0)) && buffer.ItemAt(0) != '-')
            {
                return Parse(buffer.Slice(1, buffer.Length - 1));
            }

            if (buffer.GetEndOfLinePosition() != null)
            {
                return Parse(buffer.Slice(0, buffer.Length - 2));
            }

            return ParseSequence(buffer);
        }
        
        public static long Parse(params byte[] byteValues)
        {
            return (long) byteValues.Select((t, i) => GetValue(t) * Math.Pow(10, (double) byteValues.Length - i - 1)).Sum();
        }
        
        private static long ParseSequence(ReadOnlySequence<byte> byteValues)
        {
            var result = 0d;
            var outerIndex = 0;
            foreach (var readOnlyMemory in byteValues)
            {
                foreach (var b in readOnlyMemory.Span)
                {
                    result += (char) GetValue(b) * Math.Pow(10, byteValues.Length - 1 - outerIndex);
                    outerIndex++;
                }
            }

            return (long) result;
        }
        
        private static long GetValue(byte byteValue)
        {
            return (long) char.GetNumericValue((char) byteValue);
        } 
    }
}