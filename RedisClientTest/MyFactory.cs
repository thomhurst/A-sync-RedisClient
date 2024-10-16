using System;
using System.Threading.Tasks;
using Testcontainers.Redis;
using TUnit.Core.Interfaces;

namespace RedisClientTest;

public class RedisContainerFactory : IAsyncInitializer, IAsyncDisposable
{
    public RedisContainer RedisContainer { get; } = new RedisBuilder()
        .WithImage("redis:7.4.1")
        .Build();

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
    }
}

