using System.Collections.Generic;

namespace TomLonghurst.AsyncRedisClient.Models.Commands
{
    public interface IRedisEncodable
    {
        string AsString { get; }
        IEnumerable<byte> GetEncodedCommand();
    }
}