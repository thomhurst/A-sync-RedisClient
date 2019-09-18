using System.Collections.Generic;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public interface IRedisCommand
    {
        IList<byte[]> EncodedCommandList { get; }
        string AsString { get; }
    }
}