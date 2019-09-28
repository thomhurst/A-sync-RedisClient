using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Helpers;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.Backlog;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Pipes;
#if NETSTANDARD2_0
using System.Linq;
#else
using System.Buffers;
#endif

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static readonly Logger Log = new Logger();

        private readonly SemaphoreSlim _sendAndReceiveSemaphoreSlim = new SemaphoreSlim(1, 1);
        
        private long _outStandingOperations;

        public long OutstandingOperations => Interlocked.Read(ref _outStandingOperations);

        private long _operationsPerformed;

        private SocketPipe _socketPipe;
        private PipeReader _pipeReader;
        private PipeWriter _pipeWriter;
        
        public bool IsBusy;
        
        private Thread BacklogWorkerThread;

        private string _lastAction;

        internal string LastAction
        {
            get { return _lastAction; }
            set
            {
#if DEBUG
                Console.WriteLine($"Last Action: {value}");
#endif
                _lastAction = value;
            }
        }

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        public DateTime LastUsed { get; internal set; }

        private Func<RedisTelemetryResult, Task> _telemetryCallback;
        private int _written;

        // TODO Make public
        private void SetTelemetryCallback(Func<RedisTelemetryResult, Task> telemetryCallback)
        {
            _telemetryCallback = telemetryCallback;
        }

        
        internal ValueTask<T> SendOrQueueAsync<T>(IRedisCommand command,
            ResultProcessor<T> resultProcessor,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            LastUsed = DateTime.Now;
            
#if DEBUG
            if (cancellationToken.IsCancellationRequested)
            {
                LastAction = "Throwing Cancelled Exception due to Cancelled Token";
            }
#endif
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);

            return SendAndReceiveAsync(command, resultProcessor, cancellationToken, isReconnectionAttempt);
            
//            if (isReconnectionAttempt)
//            {
//                return SendAndReceiveAsync(command, resultProcessor, cancellationToken, isReconnectionAttempt);
//            }
//
//            return QueueToBacklog(command, resultProcessor, cancellationToken);
        }

        private ValueTask<T> QueueToBacklog<T>(IRedisCommand command, ResultProcessor<T> resultProcessor,
            CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            _backlog.Enqueue(new BacklogItem<T>(command, cancellationToken, taskCompletionSource, resultProcessor, this, _pipeReader));

            cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));

            return new ValueTask<T>(taskCompletionSource.Task);
        }

        internal async ValueTask<T> SendAndReceiveAsync<T>(IRedisCommand command, ResultProcessor<T> resultProcessor,
            CancellationToken cancellationToken, bool isReconnectionAttempt)
        {
            IsBusy = true;
            
            Log.Debug($"Executing Command: {command}");
            LastCommand = command;

            Interlocked.Increment(ref _operationsPerformed);

            if (!isReconnectionAttempt)
            {
                await _sendAndReceiveSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            try
            {
                if (!isReconnectionAttempt && !IsConnected)
                {
                    await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                }

                //await ResetPipes();

                await Write(command);

                return await resultProcessor.Start(this, _pipeReader, cancellationToken);
            }
            catch (Exception innerException)
            {
                if (innerException.IsSameOrSubclassOf(typeof(RedisRecoverableException)) ||
                    innerException.IsSameOrSubclassOf(typeof(OperationCanceledException)))
                {
                    throw;
                }

                DisposeNetwork();
                
                if (innerException.IsSameOrSubclassOf(typeof(RedisNonRecoverableException)))
                {
                    throw;
                } 
                
                throw new RedisConnectionException(innerException);
            }
            finally
            {
                Interlocked.Decrement(ref _outStandingOperations);
                if (!isReconnectionAttempt)
                {
                    _sendAndReceiveSemaphoreSlim.Release();
                }
                
                IsBusy = false;
            }
        }

        private async ValueTask ResetPipes()
        {
            if (_written < 1000)
            {
                return;
            }

            _written = 0;
            
            if (_socketPipe == null)
            {
                _pipeWriter.CompleteAsync();
                _pipeReader.CompleteAsync();
                _pipeWriter = PipeWriter.Create(_sslStream, new StreamPipeWriterOptions(leaveOpen: true));
                _pipeReader = PipeReader.Create(_sslStream, new StreamPipeReaderOptions(leaveOpen: true));
            }
            else
            {
                await _pipeWriter.CompleteAsync();
                await _pipeReader.CompleteAsync();
                _socketPipe.Reset();
            }
        }

        internal async ValueTask Write(IRedisCommand command)
        {
            _written++;
            var encodedCommandList = command.EncodedCommandList;

            LastAction = "Writing Bytes";
            
            foreach (var encodedCommand in encodedCommandList)
            {
                await _pipeWriter.WriteAsync(encodedCommand);
            }
        }

        
        internal async ValueTask<T> RunWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action,
            CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();
            
            var cancellationTokenWithTimeout =
                CancellationTokenHelper.CancellationTokenWithTimeout(ClientConfig.Timeout,
                    originalCancellationToken);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                return await action.Invoke(cancellationTokenWithTimeout.Token);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw WaitTimeoutOrCancelledException(operationCanceledException, originalCancellationToken);
            }
            catch (SocketException socketException)
            {
                if (socketException.InnerException?.IsSameOrSubclassOf(typeof(OperationCanceledException)) == true)
                {
                    throw WaitTimeoutOrCancelledException(socketException.InnerException, originalCancellationToken);
                }

                throw;
            }
            finally
            {
                InvokeTelemetryCallback<T>(stopwatch);
                cancellationTokenWithTimeout.Dispose();
            }
        }

        private void InvokeTelemetryCallback<T>(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            _telemetryCallback?.Invoke(new RedisTelemetryResult("TODO", stopwatch.Elapsed));
        }

        
        private async ValueTask RunWithTimeout(Func<CancellationToken, ValueTask> action,
            CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();

            var cancellationTokenWithTimeout =
                CancellationTokenHelper.CancellationTokenWithTimeout(ClientConfig.Timeout,
                    originalCancellationToken);

            try
            {
                await action.Invoke(cancellationTokenWithTimeout.Token);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw WaitTimeoutOrCancelledException(operationCanceledException, originalCancellationToken);
            }
            catch (SocketException socketException)
            {
                if (socketException.InnerException?.IsSameOrSubclassOf(typeof(OperationCanceledException)) ==
                    true)
                {
                    throw WaitTimeoutOrCancelledException(socketException.InnerException, originalCancellationToken);
                }

                throw;
            }
            finally
            {
                cancellationTokenWithTimeout.Dispose();
            }
        }

        private Exception WaitTimeoutOrCancelledException(Exception exception, CancellationToken originalCancellationToken)
        {
            return originalCancellationToken.IsCancellationRequested ? exception : new RedisWaitTimeoutException(this);
        }
    }
}