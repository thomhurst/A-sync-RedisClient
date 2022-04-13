using System.Runtime.CompilerServices;
using System.Text;

namespace TomLonghurst.AsyncRedisClient.Extensions;

public static class MemoryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this ReadOnlyMemory<byte> memory, Memory<byte> destination, int start)
    {
        memory.CopyTo(destination.Slice(start, memory.Length));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this Memory<byte> memory, Memory<byte> destination, int start)
    {
        memory.CopyTo(destination.Slice(start, memory.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> AsReadOnlyByteMemory(this string value)
    {
        return value.ToUtf8Bytes();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> AsReadOnlyByteMemory(this int value)
    {
        return value.ToString().AsReadOnlyByteMemory();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> AsReadOnlyByteMemory(this long value)
    {
        return value.ToString().AsReadOnlyByteMemory();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> AsReadOnlyByteMemory(this float value)
    {
        return value.ToString().AsReadOnlyByteMemory();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> AsReadOnlyByteMemory(this double value)
    {
        return value.ToString().AsReadOnlyByteMemory();
    }
}