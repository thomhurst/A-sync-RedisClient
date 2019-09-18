using System.IO.Pipelines;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public class RedisPipeOptions
    {
        public PipeOptions SendOptions { get; set; }
        public PipeOptions ReceiveOptions { get; set; }
    }
}