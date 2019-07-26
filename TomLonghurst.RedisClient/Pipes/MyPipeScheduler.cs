using System;
using System.IO.Pipelines;

namespace TomLonghurst.RedisClient.Pipes
{
    public class MyPipeScheduler : PipeScheduler
    {
        public override void Schedule(Action<object> action, object state)
        {
            throw new NotImplementedException();
        }
    }
}