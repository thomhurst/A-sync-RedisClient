namespace TomLonghurst.AsyncRedisClient.Client;

public partial class RedisClient
{
    protected RedisClient()
    {
        CreateCommandClasses();

        StartBacklogProcessor();
    }

    private void CreateCommandClasses()
    {
        _clusterCommands = new ClusterCommands(this);
        _serverCommands = new ServerCommands(this);
        _scriptCommands = new ScriptCommands(this);
    }
}