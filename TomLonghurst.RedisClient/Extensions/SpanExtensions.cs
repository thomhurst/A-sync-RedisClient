using System;
using System.Buffers;
using System.Text;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class SpanExtensions
    {
        internal static unsafe SequencePosition? PositionOf(this ReadOnlySequence<byte> payload, long offset, byte value)
        {
            var sequencePosition = payload.PositionOf(value);

            if (sequencePosition == null)
            {
                return null;
            }

            return payload.GetPosition(1, sequencePosition.Value);
        }
        
        
        internal static unsafe string GetString(this ReadOnlySequence<byte> payload)
        {
            if (payload.IsEmpty) return null;

            if (payload.IsSingleSegment)
            {
                return Encoding.UTF8.GetString(payload.First.Span.ToArray());
            }
            var decoder = Encoding.UTF8.GetDecoder();
            int charCount = 0;
            foreach(var segment in payload)
            {
                var span = segment.Span;
                if (span.IsEmpty) continue;

                fixed(byte* bPtr = span)
                {
                    charCount += decoder.GetCharCount(bPtr, span.Length, false);
                }
            }

            decoder.Reset();

            string s = new string((char)0, charCount);
            fixed (char* sPtr = s)
            {
                char* cPtr = sPtr;
                foreach (var segment in payload)
                {
                    var span = segment.Span;
                    if (span.IsEmpty) continue;

                    fixed (byte* bPtr = span)
                    {
                        var written = decoder.GetChars(bPtr, span.Length, cPtr, charCount, false);
                        cPtr += written;
                        charCount -= written;
                    }
                }
            }
            return s;
        }
    }
}