using System.Net.Security;

namespace TomLonghurst.AsyncRedisClient.Client;

public record RedisClientConfig(string Host, int Port)
{
    public bool Ssl { get; init; }
    public int Db { get; init; }
    public int PoolSize { get; init; } = 50;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public string? Password { get; init; }
    public string? ClientName { get; init; }
    public RemoteCertificateValidationCallback? CertificateValidationCallback { get; init; }
    public LocalCertificateSelectionCallback? CertificateSelectionCallback { get; init; }
    
    
    public Func<RedisClient, Task>? OnConnectionEstablished { get; init; }

    public Func<RedisClient, Task>? OnConnectionFailed { get; init; }
}