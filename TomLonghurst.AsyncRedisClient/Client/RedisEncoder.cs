using System.Runtime.CompilerServices;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Helpers;

namespace TomLonghurst.AsyncRedisClient.Client;

public class RedisEncoder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1)
    {
        var digitCount = memory1.Length.GetDigitCount();

        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2)
    {
        var digitCount1 = memory1.Length.GetDigitCount();
        var digitCount2 = memory2.Length.GetDigitCount();

        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount1),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount2),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3)
    {
        var digitCount1 = memory1.Length.GetDigitCount();
        var digitCount2 = memory2.Length.GetDigitCount();
        var digitCount3 = memory3.Length.GetDigitCount();

        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount1),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount2),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount3),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4)
    {
        var digitCount1 = memory1.Length.GetDigitCount();
        var digitCount2 = memory2.Length.GetDigitCount();
        var digitCount3 = memory3.Length.GetDigitCount();
        var digitCount4 = memory4.Length.GetDigitCount();

        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount1),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount2),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount3),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount4),
            Commands.LineTerminator,
            memory4,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2, ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5)
    {
        var digitCount1 = memory1.Length.GetDigitCount();
        var digitCount2 = memory2.Length.GetDigitCount();
        var digitCount3 = memory3.Length.GetDigitCount();
        var digitCount4 = memory4.Length.GetDigitCount();
        var digitCount5 = memory5.Length.GetDigitCount();

        return ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            Commands.Number2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount1),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount2),
            Commands.LineTerminator,
            memory2,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount3),
            Commands.LineTerminator,
            memory3,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount4),
            Commands.LineTerminator,
            memory4,
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount5),
            Commands.LineTerminator,
            memory5,
            Commands.LineTerminator);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> EncodeCommand(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte>[] furtherMemorySegments)
    {
        var digitCount1 = memory1.Length.GetDigitCount();

        var endArray = furtherMemorySegments.SelectMany(furtherMemorySegment =>
        {
            var segmentDigitCount = furtherMemorySegment.Length.GetDigitCount();

            return ReadOnlyMemoryConcatenator.Concatenate(Commands.DollarSymbol,
                BitConverter.GetBytes(segmentDigitCount),
                Commands.LineTerminator,
                furtherMemorySegment,
                Commands.LineTerminator).ToArray();
        });

        var startArray = ReadOnlyMemoryConcatenator.Concatenate(Commands.AsterixSymbol,
            BitConverter.GetBytes(1 + furtherMemorySegments.Length),
            Commands.LineTerminator,
            Commands.DollarSymbol,
            BitConverter.GetBytes(digitCount1),
            Commands.LineTerminator,
            memory1,
            Commands.LineTerminator).ToArray();

        return startArray.Concat(endArray).ToArray();
    }
}