using System.Text;

namespace TomLonghurst.AsyncRedisClient.Models.Commands;

public readonly ref struct RedisCommand
{
    public byte[] EncodedBytes { get; }

    public string ConvertToString() => Encoding.UTF8.GetString(EncodedBytes);

    private RedisCommand(ReadOnlySpan<byte> input)
    {
        EncodedBytes = BytesEncoder.EncodeRawBytes(input);
    }
    
    private RedisCommand(ReadOnlySpan<byte> input1, ReadOnlySpan<byte> input2)
    {
        EncodedBytes = BytesEncoder.EncodeRawBytes(input1, input2);
    }
    
    private RedisCommand(ReadOnlySpan<byte> input1, ReadOnlySpan<byte> input2, ReadOnlySpan<byte> input3)
    {
        EncodedBytes = BytesEncoder.EncodeRawBytes(input1, input2, input3);
    }
    
    private RedisCommand(ReadOnlySpan<byte> input1, ReadOnlySpan<byte> input2, ReadOnlySpan<byte> input3, ReadOnlySpan<byte> input4)
    {
        EncodedBytes = BytesEncoder.EncodeRawBytes(input1, input2, input3, input4);
    }
    
    private RedisCommand(ReadOnlySpan<byte> input1, ReadOnlySpan<byte> input2, ReadOnlySpan<byte> input3, ReadOnlySpan<byte> input4, ReadOnlySpan<byte> input5)
    {
        EncodedBytes = BytesEncoder.EncodeRawBytes(input1, input2, input3, input4, input5);
    }
    
    public static RedisCommand From(RedisInput input)
    {
        return new RedisCommand(input.Bytes);
    }
    
    public static RedisCommand From(RedisInput input1, RedisInput input2)
    {
        return new RedisCommand(input1.Bytes, input2.Bytes);
    }
    
    public static RedisCommand From(RedisInput input1, RedisInput input2, RedisInput input3)
    {
        return new RedisCommand(input1.Bytes, input2.Bytes, input3.Bytes);
    }
    
    public static RedisCommand From(RedisInput input1, RedisInput input2, RedisInput input3, RedisInput input4)
    {
        return new RedisCommand(input1.Bytes, input2.Bytes, input3.Bytes, input4.Bytes);
    }
    
    public static RedisCommand From(RedisInput input1, RedisInput input2, RedisInput input3, RedisInput input4, RedisInput input5)
    {
        return new RedisCommand(input1.Bytes, input2.Bytes, input3.Bytes, input4.Bytes, input5.Bytes);
    }

    public static implicit operator byte[](RedisCommand command) => command.EncodedBytes;
}