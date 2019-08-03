using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Pipes;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static long _idCounter;
        public long ClientId { get; } = Interlocked.Increment(ref _idCounter);
        
        private long _reconnectAttempts;

        public long ReconnectAttempts => Interlocked.Read(ref _reconnectAttempts);

        private readonly SemaphoreSlim _connectSemaphoreSlim = new SemaphoreSlim(1, 1);

        public RedisClientConfig ClientConfig { get; }

        private readonly Timer _connectionChecker;
        
        private RedisSocket _socket;

        public Socket Socket => _socket;
        
        private SslStream _sslStream;

        private bool _isConnected;
        
        internal Action<RedisClient> OnConnectionEstablished { get; set; }
        internal Action<RedisClient> OnConnectionFailed { get; set; }

        public bool IsConnected
        {
            get
            {
                if (_socket == null || _socket.IsDisposed)
                {
                    _isConnected = false;
                }
                
                return _isConnected;
            }
            
            private set
            {
                _isConnected = value;
                
                if (!value)
                {
                    if (OnConnectionFailed != null)
                    {
                        Task.Run(() => OnConnectionFailed.Invoke(this));
                    }
                }
                else
                {
                    if (OnConnectionEstablished != null)
                    {
                        Task.Run(() => OnConnectionEstablished.Invoke(this));
                    }
                }
            }
        }

        private RedisClient(RedisClientConfig redisClientConfig) : this()
        {
            ClientConfig = redisClientConfig ?? throw new ArgumentNullException(nameof(redisClientConfig));

            _connectionChecker = new Timer(CheckConnection, null, 30000, 30000);
        }

        ~RedisClient()
        {
            Dispose();
        }
        
        private void CheckConnection(object state)
        {
            try
            {
                if (IsConnected)
                {
                    IsConnected = !(_socket.Poll(1, SelectMode.SelectRead) && _socket.Available == 0);
                }
            }
            catch (Exception)
            {
                IsConnected = false;
                DisposeNetwork();
            }
            
            if (!IsConnected)
            {
                Task.Run(() => TryConnectAsync(CancellationToken.None));
            }
        }

        internal static Task<RedisClient> ConnectAsync(RedisClientConfig redisClientConfig)
        {
            return ConnectAsync(redisClientConfig, CancellationToken.None);
        }

        internal static async Task<RedisClient> ConnectAsync(RedisClientConfig redisClientConfig, CancellationToken cancellationToken)
        {
            var redisClient = new RedisClient(redisClientConfig);
            await redisClient.TryConnectAsync(cancellationToken);
            return redisClient;
        }

        private async Task TryConnectAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
            {
                return;
            }

            try
            {
                await RunWithTimeout(async token =>
                {
                    LastAction = "Reconnecting";
                    await ConnectAsync(token);
                }, cancellationToken);
            }
            catch (Exception innerException)
            {
                IsConnected = false;
                DisposeNetwork();
                throw new RedisConnectionException(innerException);
            }
        }
        
        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
            {
                return;
            }

            LastAction = "Waiting for Connecting lock to be free";
            var wait = _connectSemaphoreSlim.WaitAsync(cancellationToken);
            if (!wait.IsCompletedSuccessfully())
            {
                await wait.ConfigureAwait(false);
            }

            try
            {
                if (IsConnected)
                {
                    return;
                }

                LastAction = "Connecting";
                Interlocked.Increment(ref _reconnectAttempts);

                _socket = new RedisSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = ClientConfig.SendTimeoutMillis,
                    ReceiveTimeout = ClientConfig.ReceiveTimeoutMillis
                };
                if (IPAddress.TryParse(ClientConfig.Host, out var ip))
                {
                    await _socket.ConnectAsync(ip, ClientConfig.Port).ConfigureAwait(false);
                }
                else
                {
                    var addresses = await Dns.GetHostAddressesAsync(ClientConfig.Host);
                    await _socket.ConnectAsync(
                        addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork),
                        ClientConfig.Port).ConfigureAwait(false);
                }


                if (!_socket.Connected)
                {
                    Log.Debug("Socket Connect failed");

                    _socket.Close();
                    _socket = null;
                    return;
                }

                _socket.NoDelay = true;

                Log.Debug("Socket Connected");

                Stream networkStream = new NetworkStream(_socket);
                
                var redisPipeOptions = GetPipeOptions();

                if (ClientConfig.Ssl)
                {
                    _sslStream = new SslStream(networkStream,
                        false,
                        ClientConfig.CertificateValidationCallback,
                        ClientConfig.CertificateSelectionCallback,
                        EncryptionPolicy.RequireEncryption);

                    LastAction = "Authenticating SSL Stream as Client";
                    await _sslStream.AuthenticateAsClientAsync(ClientConfig.Host).ConfigureAwait(false);

                    if (!_sslStream.IsEncrypted)
                    {
                        Dispose();
                        throw new SecurityException($"Could not establish an encrypted connection to Redis - {ClientConfig.Host}");
                    }

                    LastAction = "Creating SSL Stream Pipe";
                    _pipe = StreamPipe.GetDuplexPipe(_sslStream, redisPipeOptions.SendOptions, redisPipeOptions.ReceiveOptions);
                }
                else
                {
                    LastAction = "Creating Socket Pipe";
                    //_pipe = SocketConnection.Create(_socket, sendPipeOptions, receivePipeOptions);
                }

                IsConnected = true;
                
                if (!string.IsNullOrEmpty(ClientConfig.Password))
                {
                    LastAction = "Authorizing";
                    await Authorize(cancellationToken);
                }

                if (ClientConfig.Db != 0)
                {
                    LastAction = "Selecting Database";
                    await SelectDb(cancellationToken);
                }

                if (ClientConfig.ClientName != null)
                {
                    LastAction = "Setting Client Name";
                    await SetClientNameAsync(cancellationToken);
                }
            }
            finally
            {
                _connectSemaphoreSlim.Release();
            }
        }

        private static Lazy<RedisPipeOptions> Options;

        private static RedisPipeOptions GetPipeOptions()
        {
            return Options.Value;
        }

        public void Dispose()
        {
            DisposeNetwork();
            LastAction = "Disposing Client";
            _connectionChecker?.Dispose();
            _connectSemaphoreSlim?.Dispose();
        }

        private void DisposeNetwork()
        {
            IsConnected = false;
            LastAction = "Disposing Network";
            _socket?.Close();
            _socket?.Dispose();
            _sslStream?.Dispose();
        }
    }
}