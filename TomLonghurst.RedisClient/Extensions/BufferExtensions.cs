using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
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

        internal static async ValueTask<ReadResult> AdvanceToLineTerminator(this PipeReader pipeReader,
            ReadResult readResult)
        {
            readResult = await pipeReader.ReadUntilEndOfLineFound(readResult);
            var endOfLineSequencePosition = readResult.Buffer.GetEndOfLinePosition();

            if (!endOfLineSequencePosition.HasValue)
            {
                throw new Exception("Can't find EOL");
            }

            pipeReader.AdvanceTo(endOfLineSequencePosition.Value);

            return readResult;
        }

        internal static async ValueTask<ReadResult> ReadUntilEndOfLineFound(this PipeReader pipeReader, ReadResult readResult)
        {
            var buffer = readResult.Buffer;

            while (buffer.GetEndOfLinePosition() == null || !buffer.GetEndOfLinePosition().HasValue)
            {
                // We don't want to consume it yet - So don't advance past the start
                // But do tell it we've examined up until the end - But it's not enough and we need more
                // We need to call advance before calling another read though
                pipeReader.AdvanceTo(buffer.Start, buffer.End);

                if (!pipeReader.TryRead(out readResult))
                {
                    readResult = await pipeReader.ReadAsync().ConfigureAwait(false);
                }

                buffer = readResult.Buffer;
            }

            return readResult;
        }

        internal static SequencePosition? GetEndOfLinePosition(this ReadOnlySequence<byte> buffer)
        {
            var endOfLine = buffer.PositionOf((byte) '\n');

            if (!endOfLine.HasValue)
            {
                return null;
            }
            
            return buffer.GetPosition(1, endOfLine.Value);
        }
        
        internal static ArraySegment<byte> GetArraySegment(this Memory<byte> buffer) => GetArraySegment((ReadOnlyMemory<byte>)buffer);

        internal static ArraySegment<byte> GetArraySegment(this ReadOnlyMemory<byte> buffer)
        {
            if (!MemoryMarshal.TryGetArray<byte>(buffer, out var segment))
            {
                throw new InvalidOperationException("MemoryMarshal.TryGetArray<byte> could not provide an array");
            }
            
            return segment;
        }
    }
}