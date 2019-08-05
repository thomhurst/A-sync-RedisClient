using System.Collections.Generic;

namespace TomLonghurst.RedisClient.Models.Commands
{
    public interface IRedisCommand
    {
        IList<byte[]> EncodedCommandList { get; }
        string AsString { get; }
    }
}