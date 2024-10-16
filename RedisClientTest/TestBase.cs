using System;
using Testcontainers.Redis;

namespace RedisClientTest;

public class TestBase
{
    [ClassDataSource<RedisContainerFactory>(Shared = SharedType.Globally)]
    public required RedisContainerFactory ContainerFactory { get; init; }

    public Uri ConnectionString => new($"https://{RedisContainer.GetConnectionString()}");
    
    public string Host => ConnectionString.Host;
    
    public int Port => ConnectionString.Port;

    public RedisContainer RedisContainer => ContainerFactory.RedisContainer;
}