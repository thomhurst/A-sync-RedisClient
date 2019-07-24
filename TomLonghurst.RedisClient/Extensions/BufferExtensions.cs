using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class BufferExtensions
    {
        internal static string AsString(this Memory<byte> buffer)
        {
            return buffer.Span.AsString();
        }
        
        internal static string AsString(this ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsEmpty)
            {
                return null;
            }
            
            if (buffer.IsSingleSegment)
            {
                return AsString(buffer.First.Span);
            }

            var arr = ArrayPool<byte>.Shared.Rent(checked((int)buffer.Length));
            var span = new Span<byte>(arr, 0, (int)buffer.Length);
            buffer.CopyTo(span);
            var s = AsString(span);
            ArrayPool<byte>.Shared.Return(arr);
            return s;
        }

        internal static string AsString(this Span<byte> span)
        {
            return ((ReadOnlySpan<byte>) span).AsString();
        }

        internal static unsafe string AsString(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return null;
            }
            
            fixed (byte* ptr = span)
            {
                return Encoding.UTF8.GetString(ptr, span.Length);
            }
        }

        internal static async Task AdvanceToLineTerminator(this PipeReader pipeReader, ReadResult readResult)
        {
            var buffer = readResult.Buffer;

            var endOfLineSequencePosition = await pipeReader.ReadUntilEndOfLineFound(readResult);

            var sequencePositionOfLineTerminator = endOfLineSequencePosition.SequencePositionOfLineTerminator;
            
            if (sequencePositionOfLineTerminator == null)
            {
                throw new Exception("Can't find EOL");
            }
            
            buffer = buffer.Slice(sequencePositionOfLineTerminator.Value);

            pipeReader.AdvanceTo(buffer.Start, buffer.End);
        }

        internal static async Task<EndOfLineSequencePosition> ReadUntilEndOfLineFound(this PipeReader pipeReader, ReadResult readResult)
        {
            var buffer = readResult.Buffer;
            
            while ((buffer.GetEndOfLinePosition()) == null)
            {
                pipeReader.AdvanceTo(buffer.Start);

                if (!pipeReader.TryRead(out readResult))
                {
                    readResult = await pipeReader.ReadAsync();
                }

                buffer = readResult.Buffer;
            }

            return new EndOfLineSequencePosition(buffer);
        }

        internal static SequencePosition? GetEndOfLinePosition(this ReadOnlySequence<byte> buffer)
        {
            var endOfLine = buffer.PositionOf((byte) '\n');

            if (endOfLine == null)
            {
                return null;
            }
            
            return buffer.GetPosition(1, endOfLine.Value);
        }
        
        internal static int VectorSafeIndexOf(this ReadOnlySpan<byte> span, byte value)
        {
            // yes, this has zero optimization; I'm OK with this as the fallback strategy
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == value) return i;
            }
            return -1;
        }
        internal static int VectorSafeIndexOfCRLF(this ReadOnlySpan<byte> span)
        {
            // yes, this has zero optimization; I'm OK with this as the fallback strategy
            for (var i = 1; i < span.Length; i++)
            {
                if (span[i] == '\n' && span[i-1] == '\r') return i - 1;
            }
            return -1;
        }
    }
}