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
            
            if (buffer.ItemAt(0) == '$')
            {
                return Parse(buffer.Slice(1, buffer.Length - 1));
            }

            if (buffer.Length >= 2 && buffer.ItemAt(0) == '-' && buffer.ItemAt(1) == '1')
            {
                return -1;
            }
            
            switch (buffer.Length)
            {
                case 3:
                    return Parse(buffer.ItemAt(0));
                case 4:
                    return Parse(buffer.ItemAt(0), buffer.ItemAt(1));
                case 5:
                    return Parse(buffer.ItemAt(0), buffer.ItemAt(1), buffer.ItemAt(2));
                case 6:
                    return Parse(buffer.ItemAt(0), buffer.ItemAt(1), buffer.ItemAt(2), buffer.ItemAt(3));
                case 7:
                    return Parse(buffer.ItemAt(0), buffer.ItemAt(1), buffer.ItemAt(2), buffer.ItemAt(3), buffer.ItemAt(4));
                default:
                    return Parse(buffer.ToArray());
            }
        }
        
        public static long Parse(params byte[] byteValues)
        {
            return (long) byteValues.Select((t, i) => GetValue(t) * Math.Pow(10, (double) byteValues.Length - i - 1)).Sum();
        }
        
        private static long GetValue(byte byteValue)
        {
            return (long) char.GetNumericValue((char) byteValue);
        } 
    }
}