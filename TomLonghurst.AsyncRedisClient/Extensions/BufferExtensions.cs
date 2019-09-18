using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class BufferExtensions
    {
        internal static string AsString(this Memory<byte> buffer)
        {
            return buffer.Span.AsString();
        }

        internal static T ItemAt<T>(this ReadOnlySequence<T> buffer, int index)
        {
            var alreadyReadCount = 0;
            foreach (var readOnlyMemory in buffer)
            {
                if (alreadyReadCount + readOnlyMemory.Length - 1 >= index)
                {
                    return readOnlyMemory.Span[index - alreadyReadCount];
                }

                alreadyReadCount += readOnlyMemory.Length;
            }

            return default;
        }

        internal static string AsStringWithoutLineTerminators(this in ReadOnlySequence<byte> buffer)
        {
            // Reslice but removing the line terminators
            return buffer.Slice(0, buffer.Length - 2).AsString();
        }

        internal static string AsString(this in ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsEmpty)
            {
                return null;
            }

            if (buffer.IsSingleSegment)
            {
                return buffer.First.Span.AsString();
            }

            var arr = ArrayPool<byte>.Shared.Rent(checked((int) buffer.Length));
            var span = new Span<byte>(arr, 0, (int) buffer.Length);
            buffer.CopyTo(span);
            var s = span.AsString();
            ArrayPool<byte>.Shared.Return(arr);
            return s;
        }

        internal static string AsString(this in Span<byte> span)
        {
            return ((ReadOnlySpan<byte>) span).AsString();
        }

        internal static unsafe string AsString(this in ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return null;
            }

#if  NETCORE
            return Encoding.UTF8.GetString(span);
#endif

            fixed (byte* ptr = span)
            {
                return Encoding.UTF8.GetString(ptr, span.Length);
            }
        }

        internal static async ValueTask<ReadResult> AdvanceToLineTerminator(this PipeReader pipeReader,
            ReadResult readResult)
        {
            var buffer = readResult.Buffer;

            SequencePosition? endOfLinePosition;
            while ((endOfLinePosition = buffer.GetEndOfLinePosition()) == null)
            {
                // We don't want to consume it yet - So don't advance past the start
                // But do tell it we've examined up until the end - But it's not enough and we need more
                // We need to call advance before calling another read though
                pipeReader.AdvanceTo(buffer.End);

                if (!pipeReader.TryRead(out readResult))
                {
                    readResult = await pipeReader.ReadAsync().ConfigureAwait(false);
                }

                buffer = readResult.Buffer;
            }

            if (endOfLinePosition == null)
            {
                throw new RedisDataException("Can't find EOL in AdvanceToLineTerminator");
            }

            pipeReader.AdvanceTo(endOfLinePosition.Value);

            return readResult;
        }

        internal static async ValueTask<ReadResult> ReadUntilEndOfLineFound(this PipeReader pipeReader, ReadResult readResult)
        {
            while (readResult.Buffer.GetEndOfLinePosition() == null)
            {
                // We don't want to consume it yet - So don't advance past the start
                // But do tell it we've examined up until the end - But it's not enough and we need more
                // We need to call advance before calling another read though
                pipeReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);

                if (!pipeReader.TryRead(out readResult))
                {
                    readResult = await pipeReader.ReadAsync().ConfigureAwait(false);
                }
            }

            return readResult;
        }

        internal static SequencePosition? GetEndOfLinePosition(this in ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsEmpty)
            {
                return null;
            }

            var sequencePosition = buffer.PositionOf((byte) '\n');
            
            if (sequencePosition == null)
            {
                return null;
            }
            
            return buffer.GetPosition(1, sequencePosition.Value);
            //return buffer.IsSingleSegment ? GetEndOfLinePositionSingleSegment(buffer) : GetEndOfLinePositionMultipleSegments(buffer);
        }

        private static SequencePosition? GetEndOfLinePositionMultipleSegments(in ReadOnlySequence<byte> buffer)
        {
            var index = 0;
            byte lastChar = 0;
            foreach (var segment in buffer)
            {
                foreach (var b in segment.Span)
                {
                    if (b == '\n' && lastChar == '\r')
                    {
                        return buffer.GetPosition(index);
                    }

                    lastChar = b;

                    index++;
                }
            }

            return null;
        }

        private static SequencePosition? GetEndOfLinePositionSingleSegment(in ReadOnlySequence<byte> buffer)
        {
            var segment = buffer.First.Span;
            for (var i = 0; i < segment.Length; i++)
            {
                if (segment[i] == '\n' && i != 0 && segment[i - 1] == '\r')
                {
                    return buffer.GetPosition(i + 1);
                }
            }

            return null;
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