using System.Runtime.CompilerServices;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient;

internal static class BytesEncoder
{
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes)
    {

        return
        [
            ..GetPrefixLengthForBytes(bytes),
            ..bytes
        ];
    }
    
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2)
    {
        return [
            ..GetPrefixLengthForBytes(bytes1), 
            ..bytes1, 
            ..GetPrefixLengthForBytes(bytes2), 
            ..bytes2
        ];
    }
    
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2, ReadOnlySpan<byte> bytes3)
    {
        return [
            ..GetPrefixLengthForBytes(bytes1), 
            ..bytes1, 
            ..GetPrefixLengthForBytes(bytes2), 
            ..bytes2,
            ..GetPrefixLengthForBytes(bytes3), 
            ..bytes3
        ];
    }
    
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2, ReadOnlySpan<byte> bytes3, ReadOnlySpan<byte> bytes4)
    {
        return [
            ..GetPrefixLengthForBytes(bytes1), 
            ..bytes1, 
            ..GetPrefixLengthForBytes(bytes2), 
            ..bytes2,
            ..GetPrefixLengthForBytes(bytes3), 
            ..bytes3,
            ..GetPrefixLengthForBytes(bytes4), 
            ..bytes4
        ];
    }
    
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2, ReadOnlySpan<byte> bytes3, ReadOnlySpan<byte> bytes4, ReadOnlySpan<byte> bytes5)
    {
        return [
            ..GetPrefixLengthForBytes(bytes1), 
            ..bytes1, 
            ..GetPrefixLengthForBytes(bytes2), 
            ..bytes2,
            ..GetPrefixLengthForBytes(bytes3), 
            ..bytes3,
            ..GetPrefixLengthForBytes(bytes4), 
            ..bytes4,
            ..GetPrefixLengthForBytes(bytes5), 
            ..bytes5
        ];
    }
    
    public static byte[] EncodeRawBytes(byte[][] bytes)
    {
        return bytes.SelectMany(x => EncodeRawBytes(x)).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static byte[] GetPrefixLengthForBytes(ReadOnlySpan<byte> bytes)
    {
        ReadOnlySpan<char> lengthPrefix = $"${bytes.Length - 2}";

        return lengthPrefix.ToUtf8BytesWithTerminator();
    }
}