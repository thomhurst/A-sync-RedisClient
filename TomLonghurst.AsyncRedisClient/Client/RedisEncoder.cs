using System.Runtime.CompilerServices;
using System.Text;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Client;

public class RedisEncoder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1)
    {
        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2)
    {
        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory2.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3)
    {
        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number3,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory2.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory3.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4)
    {
        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number4,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory2.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory3.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory4.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory4,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5)
    {
        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number5,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory2.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory3.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory4.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory4,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory5.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory5,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte>[] furtherMemorySegments)
    {
        var endArray = furtherMemorySegments.SelectMany(furtherMemorySegment =>
        {
            return ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
                furtherMemorySegment.Length.AsReadOnlyByteMemory(),
                Commands.LineTerminator,
                furtherMemorySegment,
                Commands.LineTerminator).ToArray();
        });

        var startArray = ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            (1 + furtherMemorySegments.Length).AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator).ToArray();

        return startArray.Concat(endArray).ToArray();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte>[] furtherMemorySegments)
    {
        var endArray = furtherMemorySegments.SelectMany(furtherMemorySegment =>
        {
            return ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
                furtherMemorySegment.Length.AsReadOnlyByteMemory(),
                Commands.LineTerminator,
                furtherMemorySegment,
                Commands.LineTerminator).ToArray();
        });

        var arrayPart1 = ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            (2 + furtherMemorySegments.Length).AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator).ToArray();

        var arrayPart2 = ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
            memory2.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator).ToArray();

        return arrayPart1.Concat(arrayPart2).Concat(endArray).ToArray();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte>[] furtherMemorySegments)
    {
        var endArray = furtherMemorySegments.SelectMany(furtherMemorySegment =>
        {
            return ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
                furtherMemorySegment.Length.AsReadOnlyByteMemory(),
                Commands.LineTerminator,
                furtherMemorySegment,
                Commands.LineTerminator).ToArray();
        });

        var arrayPart1 = ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            (3 + furtherMemorySegments.Length).AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            Commands.DollarSymbol,
            memory1.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator).ToArray();

        var arrayPart2 = ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
            memory2.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator).ToArray();
        
        var arrayPart3 = ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
            memory3.Length.AsReadOnlyByteMemory(),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator).ToArray();

        return arrayPart1.Concat(arrayPart2).Concat(arrayPart3).Concat(endArray).ToArray();
    }
}