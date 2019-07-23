using System;
using System.Buffers;
using System.IO;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Helpers
{
    internal ref struct BufferReader
    {
        private ReadOnlySequence<byte>.Enumerator _iterator;
        private ReadOnlySpan<byte> _current;

        public ReadOnlySpan<byte> SlicedSpan => _current.Slice(OffsetThisSpan, RemainingThisSpan);
        public int OffsetThisSpan { get; private set; }
        private int TotalConsumed { get; set; }
        public int RemainingThisSpan { get; private set; }

        public bool IsEmpty => RemainingThisSpan == 0;

        private bool FetchNextSegment()
        {
            do
            {
                if (!_iterator.MoveNext())
                {
                    OffsetThisSpan = RemainingThisSpan = 0;
                    return false;
                }

                _current = _iterator.Current.Span;
                OffsetThisSpan = 0;
                RemainingThisSpan = _current.Length;
            } while (IsEmpty); // skip empty segments, they don't help us!

            return true;
        }

        public BufferReader(ReadOnlySequence<byte> buffer)
        {
            _buffer = buffer;
            _lastSnapshotPosition = buffer.Start;
            _lastSnapshotBytes = 0;
            _iterator = buffer.GetEnumerator();
            _current = default;
            OffsetThisSpan = RemainingThisSpan = TotalConsumed = 0;

            FetchNextSegment();
        }

        public bool TryConsume(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            do
            {
                var available = RemainingThisSpan;
                if (count <= available)
                {
                    // consume part of this span
                    TotalConsumed += count;
                    RemainingThisSpan -= count;
                    OffsetThisSpan += count;

                    if (count == available)
                    {
                        // Need to get more data
                        FetchNextSegment();
                    }

                    return true;
                }

                // consume all of this span
                TotalConsumed += available;
                count -= available;
            } while (FetchNextSegment());

            return false;
        }

        private readonly ReadOnlySequence<byte> _buffer;
        private SequencePosition _lastSnapshotPosition;
        private long _lastSnapshotBytes;

        // makes an internal note of where we are, as a SequencePosition; useful
        // to avoid having to use buffer.Slice on huge ranges
        private SequencePosition SnapshotPosition()
        {
            var consumed = TotalConsumed;
            var delta = consumed - _lastSnapshotBytes;
            if (delta == 0) return _lastSnapshotPosition;

            var pos = _buffer.GetPosition(delta, _lastSnapshotPosition);
            _lastSnapshotBytes = consumed;
            return _lastSnapshotPosition = pos;
        }

        public ReadOnlySequence<byte> ConsumeAsBuffer(int count)
        {
            if (!TryConsumeAsBuffer(count, out var buffer))
            {
                throw new EndOfStreamException();
            }

            return buffer;
        }

        public ReadOnlySequence<byte> ConsumeToEnd()
        {
            var from = SnapshotPosition();
            var result = _buffer.Slice(from);
            while (FetchNextSegment())
            {
            }

            return result;
        }

        public bool TryConsumeAsBuffer(int count, out ReadOnlySequence<byte> buffer)
        {
            var from = SnapshotPosition();

            if (!TryConsume(count))
            {
                buffer = default;
                return false;
            }

            var to = SnapshotPosition();
            buffer = _buffer.Slice(from, to);
            return true;
        }

        public void Consume(int count)
        {
            if (!TryConsume(count))
            {
                throw new EndOfStreamException();
            }
        }

        internal static int FindNextLineTerminator(BufferReader reader)
        {
            var totalSkipped = 0;
            var haveTrailingCR = false;
            do
            {
                if (reader.RemainingThisSpan == 0)
                {
                    continue;
                }

                var span = reader.SlicedSpan;
                if (haveTrailingCR)
                {
                    if (span[0] == '\n') return totalSkipped - 1;
                }

                var found = span.VectorSafeIndexOfCRLF();
                if (found >= 0)
                {
                    return totalSkipped + found;
                }

                haveTrailingCR = span[span.Length - 1] == '\r';
                totalSkipped += span.Length;
            } while (reader.FetchNextSegment());

            return -1;
        }

        public int PeekByte() => IsEmpty ? -1 : _current[OffsetThisSpan];
    }
}