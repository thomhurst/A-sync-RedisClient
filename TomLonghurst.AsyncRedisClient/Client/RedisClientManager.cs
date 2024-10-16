namespace TomLonghurst.AsyncRedisClient.Client;

public class RedisClientManager
{
    public RedisClientConfig ClientConfig { get; }
    private readonly CircularQueue<RedisClient> _redisClients;

    public static async Task<RedisClientManager> ConnectAsync(RedisClientConfig clientConfig, int redisClientPoolSize)
    {
        var manager = new RedisClientManager(clientConfig, redisClientPoolSize);

        await manager.InitializeAsync();
        
        return manager;
    }
    
    private RedisClientManager(RedisClientConfig clientConfig, int redisClientPoolSize)
    {
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
}