using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
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
            if (buffer.IsEmpty || index > buffer.Length)
            {
                return default;
            } 
            
            return buffer.Slice(buffer.GetPosition(index, buffer.Start)).First.Span[0];
        }

        internal static string AsStringWithoutLineTerminators(this in ReadOnlySequence<byte> buffer)
        {
            // Reslice but removing the line terminators
            return buffer.GetEndOfLinePosition() == null ? buffer.AsString() : buffer.Slice(buffer.Start, buffer.Length - 2).AsString();
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

#if !NETSTANDARD2_0
            return Encoding.UTF8.GetString(span);
#endif

            fixed (byte* ptr = span)
            {
                return Encoding.UTF8.GetString(ptr, span.Length);
            }
        }

        internal static async ValueTask<ReadResult> AdvanceToLineTerminator(this PipeReader pipeReader,
            ReadResult readResult, CancellationToken cancellationToken)
        {
            var buffer = readResult.Buffer;

            SequencePosition? endOfLinePosition;
            while ((endOfLinePosition = buffer.GetEndOfLinePosition()) == null)
            {
                if (readResult.IsCompleted && readResult.Buffer.IsEmpty)
                {
                    throw new RedisDataException("ReadResult is completed and buffer is empty. Can't find EOL in AdvanceToLineTerminator");
                }
                
                if (readResult.IsCanceled)
                {
                    throw new RedisDataException("ReadResult is cancelled. Can't find EOL in AdvanceToLineTerminator");
                }
                
                pipeReader.AdvanceTo(buffer.End);

                if (!pipeReader.TryRead(out readResult))
                {
                    readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken).ConfigureAwait(false);
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

        internal static async ValueTask<ReadResult> ReadUntilEndOfLineFound(this PipeReader pipeReader, ReadResult readResult, CancellationToken cancellationToken)
        {
            while (readResult.Buffer.GetEndOfLinePosition() == null)
            {
                if (readResult.IsCompleted && readResult.Buffer.IsEmpty)
                {
                    break;
                }

                if (readResult.IsCanceled)
                {
                    break;
                }

                // We don't want to consume it yet - So don't advance past the start
                // But do tell it we've examined up until the end - But it's not enough and we need more
                // We need to call advance before calling another read though
                pipeReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);

                if (!pipeReader.TryRead(out readResult))
                {
                    readResult = await pipeReader.ReadAsyncOrThrowReadTimeout(cancellationToken).ConfigureAwait(false);
                }
            }

            if (readResult.Buffer.GetEndOfLinePosition() == null)
            {
                throw new RedisDataException("No EOL found while executing ReadUntilEndOfLineFound");
            }

            return readResult;
        }

        internal static SequencePosition? GetEndOfLinePosition2(this in ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsEmpty)
            {
                throw new RedisDataException("The buffer is empty in GetEndOfLinePosition");
            }

            var sequencePosition = buffer.PositionOf(ByteConstants.NewLine);
            
            if (sequencePosition == null)
            {
                return null;
            }
            
            return buffer.GetPosition(1, sequencePosition.Value);
        }

        internal static SequencePosition? GetEndOfLinePosition(this in ReadOnlySequence<byte> buffer)
        {
            var position = buffer.Start;
            var previous = position;
            var index = -1;

            while (buffer.TryGet(ref position, out var segment))
            {
                var span = segment.Span;

                // Look for \r in the current segment
                index = span.IndexOf(ByteConstants.BackslashR);

                if (index != -1)
                {
                    // Check next segment for \n
                    if (index + 1 >= span.Length)
                    {
                        var next = position;
                        
                        if (!buffer.TryGet(ref next, out var nextSegment))
                        {
                            // We're at the end of the sequence
                            return null;
                        }

                        if (nextSegment.Span[0] == ByteConstants.NewLine)
                        {
                            //  We found a match
                            break;
                        }
                    }
                    // Check the current segment of \n
                    else if (span[index + 1] == ByteConstants.NewLine)
                    {
                        // Found it
                        break;
                    }
                }

                previous = position;
            }

            if (index != -1)
            {
                return buffer.GetPosition(index + 2, previous);
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