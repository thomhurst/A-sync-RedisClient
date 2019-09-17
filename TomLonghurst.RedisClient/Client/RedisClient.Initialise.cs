namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        //private DedicatedScheduler _backlogScheduler = new DedicatedScheduler(workerCount: 1);

        protected RedisClient()
        {
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