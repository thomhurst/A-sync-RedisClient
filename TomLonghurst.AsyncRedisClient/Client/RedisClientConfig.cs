using System.Net.Security;

namespace TomLonghurst.AsyncRedisClient.Client;

public record RedisClientConfig(string Host, int Port)
{
    public bool Ssl { get; init; }
    public int Db { get; init; }
    public int SendTimeoutMillis { get; init; } = 5000;
    public int ReceiveTimeoutMillis { get; init; } = 5000;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);
    public string? Password { get; init; }
    public string? ClientName { get; init; }
    public RemoteCertificateValidationCallback? CertificateValidationCallback { get; init; }
    public LocalCertificateSelectionCallback? CertificateSelectionCallback { get; init; }
    
    
    public Func<RedisClient, Task>? OnConnectionEstablished { get; init; }

    public Func<RedisClient, Task>? OnConnectionFailed { get; init; }
}