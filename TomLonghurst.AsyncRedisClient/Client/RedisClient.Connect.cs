using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Pipes;

namespace TomLonghurst.AsyncRedisClient.Client;

public partial class RedisClient : IDisposable
{
    private static long _idCounter;
    public long ClientId { get; } = Interlocked.Increment(ref _idCounter);
        
    private long _reconnectAttempts;

    public long ReconnectAttempts => Interlocked.Read(ref _reconnectAttempts);

    private readonly SemaphoreSlim _connectSemaphoreSlim = new(1, 1);

    public RedisClientConfig ClientConfig { get; }

    private RedisSocket? _socket;

    public Socket? Socket => _socket;
        
    private SslStream? _sslStream;

    private bool _isConnected;
        
    internal Func<RedisClient, Task>? OnConnectionEstablished { get; set; }
    internal Func<RedisClient, Task>? OnConnectionFailed { get; set; }
        
    public bool IsConnected
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            if (_socket == null || _socket.IsDisposed || !_socket.Connected || _socket.IsClosed)
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

    protected RedisClient(RedisClientConfig redisClientConfig)
    {
        ClientConfig = redisClientConfig ?? throw new ArgumentNullException(nameof(redisClientConfig));

        Cluster = new ClusterCommands(this);
        Server = new ServerCommands(this);
        Scripts = new ScriptCommands(this);
        
        StartBacklogProcessor();
        
        _connectionChecker = new Timer(CheckConnection, null, 2500, 250);
    }

    ~RedisClient()
    {
        Dispose();
    }
        
    private void CheckConnection(object? state)
    {
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

        await _connectSemaphoreSlim.WaitAsync(cancellationToken);

        if (IsConnected)
        {
            _connectSemaphoreSlim.Release();
            return;
        }
            
        try
        {
            Interlocked.Increment(ref _reconnectAttempts);

            _socket = new RedisSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = ClientConfig.Timeout.Milliseconds,
                ReceiveTimeout = ClientConfig.Timeout.Milliseconds
            };
                
            OptimiseSocket();
                
            if (IPAddress.TryParse(ClientConfig.Host, out var ip))
            {
                await _socket.ConnectAsync(ip, ClientConfig.Port, cancellationToken);
            }
            else
            {
                var addresses = await Dns.GetHostAddressesAsync(ClientConfig.Host, cancellationToken);
                await _socket.ConnectAsync(
                    addresses.First(a => a.AddressFamily == AddressFamily.InterNetwork),
                    ClientConfig.Port, cancellationToken);
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

                // TODO
                // await _sslStream.AuthenticateAsClientAsync(ClientConfig.Host);

                if (!_sslStream.IsEncrypted)
                {
                    DisposeNetwork();
                    throw new SecurityException($"Could not establish an encrypted connection to Redis - {ClientConfig.Host}");
                }

                _pipeWriter = PipeWriter.Create(_sslStream, new StreamPipeWriterOptions(leaveOpen: true));
                _pipeReader = PipeReader.Create(_sslStream, new StreamPipeReaderOptions(leaveOpen: true));
            }
            else
            {
                _socketPipe = SocketPipe.GetDuplexPipe(_socket, redisPipeOptions.SendOptions ?? new PipeOptions(), redisPipeOptions.ReceiveOptions ?? new PipeOptions());
                _pipeWriter = _socketPipe.Output;
                _pipeReader = _socketPipe.Input;
            }

            if (!string.IsNullOrEmpty(ClientConfig.Password))
            {
                await Authorize(cancellationToken);
            }

            if (ClientConfig.Db != 0)
            {
                await SelectDb(cancellationToken);
            }

            if (ClientConfig.ClientName != null)
            {
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
        if (_socket!.AddressFamily == AddressFamily.Unix)
        {
            return;
        }

        try
        {
            _socket.NoDelay = true;
        }
        catch
        {
            // If we can't set this, just continue - There's nothing we can do!
        }
    }
        
    private readonly Timer? _connectionChecker;
    private bool _disposed;

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
        _connectSemaphoreSlim?.Dispose();
        _sendAndReceiveSemaphoreSlim?.Dispose();
        _backlog?.Dispose();
        _connectionChecker?.Dispose();
    }

    private void DisposeNetwork()
    {
        IsConnected = false;
        _pipeReader?.CompleteAsync();
        _pipeWriter?.CompleteAsync();
        _socket?.Close();
        _socket?.Dispose();
        _sslStream?.Dispose();
    }
}