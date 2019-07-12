using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial;
using TomLonghurst.RedisClient.Exceptions;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static long _idCounter;
        public long ClientId { get; } = Interlocked.Increment(ref _idCounter);
        
        private long _reconnectAttempts;

        public long ReconnectAttempts => Interlocked.Read(ref _reconnectAttempts);

        private readonly SemaphoreSlim _connectSemaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly RedisClientConfig _redisClientConfig;
        
        private readonly Timer _connectionChecker;
        
        private RedisSocket _socket;

        public Socket Socket => _socket;

        private BufferedStream _bufferedStream;
        private SslStream _sslStream;
        
        private const int BufferSize = 16 * 1024;

        private bool _isConnected;
        private IDuplexPipe pipe;

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
            private set => _isConnected = value;
        }

        private RedisClient(RedisClientConfig redisClientConfig)
        {
            _redisClientConfig = redisClientConfig ?? throw new ArgumentNullException(nameof(redisClientConfig));

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
                    IsConnected = !(_socket.Poll(1000, SelectMode.SelectRead) && _socket.Available == 0);
                }
            }
            catch (Exception)
            {
                IsConnected = false;
                DisposeNetwork();
            }
            
            if (!IsConnected)
            {
#pragma warning disable 4014
                TryConnectAsync(CancellationToken.None);
#pragma warning restore 4014
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

            await _connectSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (IsConnected)
                {
                    return;
                }

                Interlocked.Increment(ref _reconnectAttempts);

                _socket = new RedisSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = _redisClientConfig.SendTimeout,
                    ReceiveTimeout = _redisClientConfig.ReceiveTimeout
                };
                
                SocketConnection.SetRecommendedClientOptions(_socket);
                
                if (IPAddress.TryParse(_redisClientConfig.Host, out var ip))
                {
                    await _socket.ConnectAsync(ip, _redisClientConfig.Port);
                }
                else
                {
                    var addresses = await Dns.GetHostAddressesAsync(_redisClientConfig.Host);
                    await _socket.ConnectAsync(
                        addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork),
                        _redisClientConfig.Port);
                }


                if (!_socket.Connected)
                {
                    Log.Debug("Socket Connect failed");

                    _socket.Close();
                    _socket = null;
                    return;
                }

                Log.Debug("Socket Connected");

                Stream networkStream = new NetworkStream(_socket);

                if (_redisClientConfig.Ssl)
                {
                    _sslStream = new SslStream(networkStream,
                        false,
                        _redisClientConfig.CertificateValidationCallback,
                        _redisClientConfig.CertificateSelectionCallback,
                        EncryptionPolicy.RequireEncryption);

                    await _sslStream.AuthenticateAsClientAsync(_redisClientConfig.Host);

                    if (!_sslStream.IsEncrypted)
                    {
                        Dispose();
                        throw new SecurityException($"Could not establish an encrypted connection to Redis - {_redisClientConfig.Host}");
                    }

                    networkStream = _sslStream;

                    pipe = StreamConnection.GetDuplex(_sslStream, SendPipeOptions, ReceivePipeOptions);
                }
                else
                {
                    pipe = SocketConnection.Create(_socket, SendPipeOptions, ReceivePipeOptions);
                }

                _bufferedStream = new BufferedStream(networkStream, BufferSize);

                IsConnected = true;
                
                if (!string.IsNullOrEmpty(_redisClientConfig.Password))
                {
                    await Authorize(cancellationToken);
                }

                if (_redisClientConfig.Db != 0)
                {
                    await SelectDb(cancellationToken);
                }

                if (_redisClientConfig.ClientName != null)
                {
                    await SetClientName(cancellationToken);
                }
            }
            finally
            {
                _connectSemaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            DisposeNetwork();
            _connectionChecker?.Dispose();
            _sendAndReceiveSemaphoreSlim?.Dispose();
            _connectSemaphoreSlim?.Dispose();
        }

        private void DisposeNetwork()
        {
            _socket?.Close();
            _socket?.Dispose();
            _bufferedStream?.Dispose();
            _sslStream?.Dispose();
        }
    }
}