using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Pipes;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static long _idCounter;
        public long ClientId { get; } = Interlocked.Increment(ref _idCounter);
        
        private long _reconnectAttempts;

        public long ReconnectAttempts => Interlocked.Read(ref _reconnectAttempts);

        private readonly SemaphoreSlim _connectSemaphoreSlim = new SemaphoreSlim(1, 1);

        public RedisClientConfig ClientConfig { get; }

        private RedisSocket _socket;

        public Socket Socket => _socket;
        
        private SslStream _sslStream;

        private bool _isConnected;
        
        internal Func<RedisClient, Task> OnConnectionEstablished { get; set; }
        internal Func<RedisClient, Task> OnConnectionFailed { get; set; }
        
        public bool IsConnected
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_socket == null || _socket.IsDisposed || !_socket.Connected)
                {
                    _isConnected = false;
                }
                
                return _isConnected;
            }
            
            [MethodImpl(MethodImplOptions.Synchronized)]
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

        protected RedisClient(RedisClientConfig redisClientConfig) : this()
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
            if (!IsConnected && !_disposed)
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
            await _connectSemaphoreSlim.WaitAsync(cancellationToken);

            if (IsConnected)
            {
                _connectSemaphoreSlim.Release();
                return;
            }
            
            try
            {
                LastAction = "Connecting";
                Interlocked.Increment(ref _reconnectAttempts);

                _socket = new RedisSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = ClientConfig.SendTimeoutMillis,
                    ReceiveTimeout = ClientConfig.ReceiveTimeoutMillis
                };
                
                OptimiseSocket();
                
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

                    DisposeNetwork();
                    return;
                }

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
                        DisposeNetwork();
                        throw new SecurityException($"Could not establish an encrypted connection to Redis - {ClientConfig.Host}");
                    }

                    LastAction = "Creating SSL Stream Pipe";
                    _pipeWriter = PipeWriter.Create(_sslStream, new StreamPipeWriterOptions(leaveOpen: true));
                    _pipeReader = PipeReader.Create(_sslStream, new StreamPipeReaderOptions(leaveOpen: true));
                }
                else
                {
                    LastAction = "Creating Socket Pipe";
                    _socketPipe = SocketPipe.GetDuplexPipe(_socket, redisPipeOptions.SendOptions, redisPipeOptions.ReceiveOptions);
                    _pipeWriter = _socketPipe.Output;
                    _pipeReader = _socketPipe.Input;
                }

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

                IsConnected = true;
            }
            finally
            {
                _connectSemaphoreSlim.Release();
            }
        }

        private void OptimiseSocket()
        {
            if (_socket.AddressFamily == AddressFamily.Unix)
            {
                return;
            }

            try { _socket.NoDelay = true; } catch { }
        }
        
        private bool _disposed;
        private readonly Timer _connectionChecker;

        private static RedisPipeOptions GetPipeOptions()
        {
            const int defaultMinimumSegmentSize = 4 * 16;

            const long sendPauseWriterThreshold = 512 * 1024;
            const long sendResumeWriterThreshold = sendPauseWriterThreshold / 2;

            const long receivePauseWriterThreshold = 1024 * 1024 * 1024;
            const long receiveResumeWriterThreshold = receivePauseWriterThreshold / 2;

            var scheduler = PipeScheduler.ThreadPool;
            var defaultPipeOptions = PipeOptions.Default;

            var receivePipeOptions = new PipeOptions(
                defaultPipeOptions.Pool,
                scheduler,
                scheduler,
                receivePauseWriterThreshold,
                receiveResumeWriterThreshold,
                defaultMinimumSegmentSize,
                false);

            var sendPipeOptions = new PipeOptions(
                defaultPipeOptions.Pool,
                scheduler,
                scheduler,
                sendPauseWriterThreshold,
                sendResumeWriterThreshold,
                defaultMinimumSegmentSize,
                false);

            return new RedisPipeOptions
            {
                SendOptions = sendPipeOptions,
                ReceiveOptions = receivePipeOptions
            };
        }

        public void Dispose()
        {
            _disposed = true;
            DisposeNetwork();
            LastAction = "Disposing Client";
            _connectSemaphoreSlim?.Dispose();
            _sendAndReceiveSemaphoreSlim?.Dispose();
            _backlog?.Dispose();
            _connectionChecker?.Dispose();
        }

        private void DisposeNetwork()
        {
            IsConnected = false;
            LastAction = "Disposing Network";
            _pipeReader?.CompleteAsync();
            _pipeWriter?.CompleteAsync();
            _socket?.Close();
            _socket?.Dispose();
            _sslStream?.Dispose();
        }
    }
}