using System.Runtime.CompilerServices;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient;

internal static class BytesEncoder
{
    private static readonly byte[] OneElement = ((ReadOnlySpan<char>)"*1").ToUtf8BytesWithTerminator();
    private static readonly byte[] TwoElements = ((ReadOnlySpan<char>)"*2").ToUtf8BytesWithTerminator();
    private static readonly byte[] ThreeElements = ((ReadOnlySpan<char>)"*3").ToUtf8BytesWithTerminator();
    private static readonly byte[] FourElements = ((ReadOnlySpan<char>)"*4").ToUtf8BytesWithTerminator();
    private static readonly byte[] FiveElements = ((ReadOnlySpan<char>)"*5").ToUtf8BytesWithTerminator();
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes)
    {

        return
        [
            ..OneElement,
            ..GetPrefixLengthForBytes(bytes),
            ..bytes
        ];
    }
    
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2)
    {
        return [
            ..TwoElements,
            ..GetPrefixLengthForBytes(bytes1), 
            ..bytes1, 
            ..GetPrefixLengthForBytes(bytes2), 
            ..bytes2
        ];
    }
    
    public static byte[] EncodeRawBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2, ReadOnlySpan<byte> bytes3)
    {
        return [
            ..ThreeElements,
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
            ..FourElements,
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
            ..FiveElements,
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