using System.Runtime.CompilerServices;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Helpers;

public class ReadOnlyMemoryConcatenator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2)
    {
        var array = new byte[memory1.Length + memory2.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        return array;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17, ReadOnlyMemory<byte> memory18)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        memory18.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length, memory18.Length));
        return array;
    }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17, ReadOnlyMemory<byte> memory18, ReadOnlyMemory<byte> memory19)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        memory18.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length, memory18.Length));
        memory19.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length, memory19.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17, ReadOnlyMemory<byte> memory18, ReadOnlyMemory<byte> memory19, ReadOnlyMemory<byte> memory20)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        memory18.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length, memory18.Length));
        memory19.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length, memory19.Length));
        memory20.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length, memory20.Length));
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17, ReadOnlyMemory<byte> memory18, ReadOnlyMemory<byte> memory19, ReadOnlyMemory<byte> memory20, ReadOnlyMemory<byte> memory21)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length + memory21.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        memory18.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length, memory18.Length));
        memory19.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length, memory19.Length));
        memory20.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length, memory20.Length));
        memory21.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length, memory21.Length));
        return array;
    }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17, ReadOnlyMemory<byte> memory18, ReadOnlyMemory<byte> memory19, ReadOnlyMemory<byte> memory20, ReadOnlyMemory<byte> memory21, ReadOnlyMemory<byte> memory22)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length + memory21.Length + memory22.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        memory18.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length, memory18.Length));
        memory19.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length, memory19.Length));
        memory20.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length, memory20.Length));
        memory21.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length, memory21.Length));
        memory22.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length + memory21.Length, memory22.Length));
        return array;
    }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10, ReadOnlyMemory<byte> memory11, ReadOnlyMemory<byte> memory12, ReadOnlyMemory<byte> memory13, ReadOnlyMemory<byte> memory14, ReadOnlyMemory<byte> memory15, ReadOnlyMemory<byte> memory16, ReadOnlyMemory<byte> memory17, ReadOnlyMemory<byte> memory18, ReadOnlyMemory<byte> memory19, ReadOnlyMemory<byte> memory20, ReadOnlyMemory<byte> memory21, ReadOnlyMemory<byte> memory22, ReadOnlyMemory<byte> memory23)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length + memory21.Length + memory22.Length + memory23.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory3.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory4.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory5.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory6.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory7.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory8.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory9.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory10.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        memory11.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length, memory11.Length));
        memory12.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length, memory12.Length));
        memory13.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length, memory13.Length));
        memory14.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length, memory14.Length));
        memory15.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length, memory15.Length));
        memory16.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length, memory16.Length));
        memory17.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length, memory17.Length));
        memory18.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length, memory18.Length));
        memory19.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length, memory19.Length));
        memory20.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length, memory20.Length));
        memory21.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length, memory21.Length));
        memory22.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length + memory21.Length, memory22.Length));
        memory23.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length + memory11.Length + memory12.Length + memory13.Length + memory14.Length + memory15.Length + memory16.Length + memory17.Length + memory18.Length + memory19.Length + memory20.Length + memory21.Length + memory22.Length, memory23.Length));
        return array;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static ReadOnlyMemory<byte> Concatenate(params ReadOnlyMemory<byte>[] memorySegments)
    // {
    //     var byteCount = memorySegments.Select(x => x.Length).Sum();
    //     var array = new byte[byteCount].AsMemory();
    //
    //     var written = 0;
    //     foreach (var memorySegment in memorySegments)
    //     {
    //         memorySegment.CopyTo(array, written);
    //         written += memorySegment.Length;
    //     }
    //
    //     return array;
    // }
}