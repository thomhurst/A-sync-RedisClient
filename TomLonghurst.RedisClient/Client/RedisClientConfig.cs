using System.Net.Security;

namespace TomLonghurst.RedisClient.Client
{
    public class RedisClientConfig
    {
        public RedisClientConfig(string host, int port) : this(host, port, null)
        {
            
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
        public int SendTimeout { get; set; } = 5000;
        public int ReceiveTimeout { get; set; } = 5000;
        public int Timeout { get; set; } = 5000;
        public string Password { get; set; }
        public RemoteCertificateValidationCallback CertificateValidationCallback { get; set; }
        public LocalCertificateSelectionCallback CertificateSelectionCallback { get; set; }
    }
}
