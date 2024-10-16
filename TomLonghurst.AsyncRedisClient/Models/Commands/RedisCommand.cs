using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Models.Commands;

public class RedisCommand : IRedisCommand
{
    internal readonly IEnumerable<IRedisEncodable> RedisEncodables;
    internal readonly byte[][] RawBytes;

    private byte[][]? _encodedCommand;

    public IList<byte[]>? EncodedCommandList
    {
        get
        {
            if (_encodedCommand != null)
            {
                return _encodedCommand;
            }

            _encodedCommand =
            [
                $"*{RawBytes.Length}".ToUtf8BytesWithTerminator(),
                ..EncodeRawBytes()
            ];

            return _encodedCommand;
        }
    }

    private IEnumerable<byte[]> EncodeRawBytes()
    {
        foreach (var rawByte in RawBytes)
        {
            yield return $"${rawByte.Length - 2}".ToUtf8BytesWithTerminator();
            yield return rawByte;
        }
    }

    public string AsString => string.Join(" ", RedisEncodables.Select(x => x.AsString));

    private RedisCommand(IReadOnlyCollection<IRedisEncodable> redisEncodables)
    {
        RedisEncodables = redisEncodables;
        RawBytes = redisEncodables.Select(x => x.RedisEncodedBytes).ToArray();
    }
        
    public static RedisCommand From(IReadOnlyCollection<IRedisEncodable> redisEncodables)
    {
        return new RedisCommand(redisEncodables);
    }
        
    private RedisCommand(params IRedisEncodable[] redisEncodables)
    {
        RedisEncodables = redisEncodables;
        RawBytes = redisEncodables.Select(x => x.RedisEncodedBytes).ToArray();
    }
        
    public static RedisCommand From(params IRedisEncodable[] redisEncodables)
    {
        return new RedisCommand(redisEncodables);
    }
        
    public static RedisCommand FromScript(IRedisEncodable command, IRedisEncodable scriptOrSha1, List<string> keys, IEnumerable<string> args)
    {
        IRedisEncodable[] encodables =
        [
            command,
            scriptOrSha1,
            keys.Count.ToRedisEncoded(),
            ..keys.Select(key => key.ToRedisEncoded()),
            ..args.Select(arg => arg.ToRedisEncoded())
        ];
        
        return new RedisCommand(encodables);
    }

    private RedisCommand(IRedisEncodable redisEncodable)
    {
        RedisEncodables = [redisEncodable];
        RawBytes = [redisEncodable.RedisEncodedBytes];
    }

    public static RedisCommand From(IRedisEncodable redisEncodable)
    {
        return new RedisCommand(redisEncodable);
    }

    public static IRedisCommand From(IRedisEncodable redisEncodable, IEnumerable<IRedisEncodable> redisEncodables)
    {
        return new RedisCommand([redisEncodable, ..redisEncodables]);
    }
}