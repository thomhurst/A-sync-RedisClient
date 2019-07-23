using System;
using System.Buffers;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient
{
    public class EndOfLineSequencePosition
    {
        public SequencePosition SequencePositionBeforeLineTerminator { get; }
        public SequencePosition SequencePositionOfLineTerminator { get; }

        public EndOfLineSequencePosition(ReadOnlySequence<byte> buffer)
        {
            SequencePositionOfLineTerminator = buffer.GetEndOfLinePosition().Value;
            SequencePositionBeforeLineTerminator = buffer.GetPosition(SequencePositionOfLineTerminator.GetInteger() - 2);
        }
    }
}