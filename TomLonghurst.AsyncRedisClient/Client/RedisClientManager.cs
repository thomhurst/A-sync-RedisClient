namespace TomLonghurst.AsyncRedisClient.Client;

public class RedisClientManager : IAsyncDisposable
{
    public RedisClientConfig? ClientConfig { get; }
    private readonly CircularQueue<RedisClient> _redisClients;

    public static async Task<RedisClientManager> ConnectAsync(RedisClientConfig? clientConfig)
    {
        var manager = new RedisClientManager(clientConfig);

        await manager.InitializeAsync();
        
        return manager;
    }
    
    private RedisClientManager(RedisClientConfig? clientConfig)
    {
        var redisClientPoolSize = clientConfig.PoolSize;
        
        if (redisClientPoolSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(redisClientPoolSize), "Pool size must be 1 or more");
        }

        ClientConfig = clientConfig;

        _redisClients = new(async () => await RedisClient.ConnectAsync(clientConfig), redisClientPoolSize);
    }

    private Task InitializeAsync()
    {
        return _redisClients.InitializeAsync();
    }

    public RedisClient GetRedisClient()
    {
        return _redisClients.Get();
    }

    public ValueTask DisposeAsync()
    {
        return _redisClients.DisposeAsync();
    }
}