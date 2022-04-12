namespace TomLonghurst.AsyncRedisClient.Helpers;

public class ReadOnlyMemoryConcatenator
{
    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2)
    {
        var array = new byte[memory1.Length + memory2.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        return array;
    }

    public static ReadOnlyMemory<byte> Concatenate(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2,
        ReadOnlyMemory<byte> memory3, ReadOnlyMemory<byte> memory4, ReadOnlyMemory<byte> memory5,
        ReadOnlyMemory<byte> memory6, ReadOnlyMemory<byte> memory7, ReadOnlyMemory<byte> memory8,
        ReadOnlyMemory<byte> memory9, ReadOnlyMemory<byte> memory10)
    {
        var array = new byte[memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length + memory10.Length].AsMemory();
        memory1.CopyTo(array.Slice(0, memory1.Length));
        memory2.CopyTo(array.Slice(memory1.Length, memory2.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length, memory3.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length, memory4.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length, memory5.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length, memory6.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length, memory7.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length, memory8.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length, memory9.Length));
        memory2.CopyTo(array.Slice(memory1.Length + memory2.Length + memory3.Length + memory4.Length + memory5.Length + memory6.Length + memory7.Length + memory8.Length + memory9.Length, memory10.Length));
        return array;
    }
}