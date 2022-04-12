using System.Net.Security;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public class RedisClientConfig
    {
        public RedisClientConfig(string host, int port)
        {
            Host = host;
            Port = port;
        }
        
        public RedisClientConfig(string host, int port, string password)
        {
            Host = host;
            Port = port;
            Password = password;
        }

        public string Host { get; }
        public int Port { get; }
        public bool Ssl { get; set; }
        public int Db { get; set; }
        public int SendTimeoutMillis { get; set; } = 5000;
        public int ReceiveTimeoutMillis { get; set; } = 5000;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
        public string Password { get; }
        public string ClientName { get; set; }
        public RemoteCertificateValidationCallback CertificateValidationCallback { get; set; }
        public LocalCertificateSelectionCallback CertificateSelectionCallback { get; set; }
    }
}
