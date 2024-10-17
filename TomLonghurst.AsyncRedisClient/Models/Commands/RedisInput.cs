using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Models.Commands;

public ref struct RedisInput
{
    public byte[] Bytes { get; }

    public RedisInput(ReadOnlySpan<char> input)
    {
        var byteCount = Encoding.UTF8.GetByteCount(input);
        Bytes = new byte[byteCount + 2];
        Encoding.UTF8.GetBytes(input, Bytes);
        Bytes[^2] = ByteConstants.BackslashR;
        Bytes[^1] = ByteConstants.NewLine;
    }
    
    public RedisInput(byte[] input)
    {
        Bytes = input;
    }
    
    public RedisInput(int input)
    {
        Bytes = BitConverter.GetBytes(input);
    }
    
    public RedisInput(long input)
    {
        Bytes = BitConverter.GetBytes(input);
    }
    
    public RedisInput(double input)
    {
        Bytes = BitConverter.GetBytes(input);
    }
    
    public RedisInput(float input)
    {
        Bytes = BitConverter.GetBytes(input);
    }
    
    public RedisInput(IEnumerable<string> inputs)
    {
        Bytes = inputs.SelectMany(x => new RedisInput(x).Bytes).ToArray();
    }
    
    public static implicit operator RedisInput(Span<char> input) => new(input); 
    public static implicit operator RedisInput(string input) => new(input); 
    public static implicit operator RedisInput(byte[] input) => new(input); 
    public static implicit operator RedisInput(int input) => new(input); 
    public static implicit operator RedisInput(long input) => new(input); 
    public static implicit operator RedisInput(float input) => new(input); 
    public static implicit operator RedisInput(double input) => new(input); 
    public static implicit operator RedisInput(string[] inputs) => new(inputs); 
    public static implicit operator RedisInput(List<string> inputs) => new(inputs); 
    public static implicit operator RedisInput(Collection<string> inputs) => new(inputs); 
    public static implicit operator RedisInput(ImmutableArray<string> inputs) => new(inputs); 
    public static implicit operator RedisInput(ImmutableList<string> inputs) => new(inputs); 
}