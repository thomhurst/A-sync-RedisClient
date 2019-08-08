using System;
using TomLonghurst.RedisClient.Pipes;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        //private DedicatedScheduler _backlogScheduler = new DedicatedScheduler(workerCount: 1);

        protected RedisClient()
        {
            _weakReference = new WeakReference<RedisClient>(this);
            CreateCommandClasses();

            StartBacklogProcessor();
        }

        private void CreateCommandClasses()
        {
            _clusterCommands = new ClusterCommands(this);
            _serverCommands = new ServerCommands(this);
        }
    }
}