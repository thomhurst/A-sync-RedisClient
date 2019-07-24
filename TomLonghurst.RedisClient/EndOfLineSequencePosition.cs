using System;
using System.Buffers;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Helpers;

namespace TomLonghurst.RedisClient
{
    public class EndOfLineSequencePosition
    {
        public int PositionBeforeLineTerminator { get; }
        public SequencePosition? SequencePositionOfLineTerminator { get; }

        public EndOfLineSequencePosition(ReadOnlySequence<byte> buffer)
        {
            var bufferReader = new BufferReader(buffer);
            
            SequencePositionOfLineTerminator = buffer.GetEndOfLinePosition();
            PositionBeforeLineTerminator = BufferReader.FindNextLineTerminator(bufferReader);
        }
    }
}