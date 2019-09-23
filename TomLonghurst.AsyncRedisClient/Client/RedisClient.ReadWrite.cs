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
#if NETSTANDARD
using System.Linq;
#endif

#if NETCORE
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

        internal string LastAction;

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        public DateTime LastUsed { get; internal set; }

        private Func<RedisTelemetryResult, Task> _telemetryCallback;

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

            LastAction = "Throwing Cancelled Exception due to Cancelled Token";
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

                await ResetPipes();

                await Write(command);

                LastAction = "Reading Bytes Async";

                return await resultProcessor.Start(this, _pipeReader);
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

        internal ValueTask<FlushResult> Write(IRedisCommand command)
        {
            var encodedCommandList = command.EncodedCommandList;

            LastAction = "Writing Bytes";
            
            var task = _pipeWriter.WriteAsync(encodedCommandList.SelectMany(x => x).ToArray().AsMemory());

            if (!task.IsCompleted)
            {
                return task;
            }

            return default;
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
                throw TimeoutOrCancelledException(operationCanceledException, originalCancellationToken);
            }
            catch (SocketException socketException)
            {
                if (socketException.InnerException?.GetType().IsAssignableFrom(typeof(OperationCanceledException)) ==
                    true)
                {
                    throw TimeoutOrCancelledException(socketException.InnerException, originalCancellationToken);
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
                throw TimeoutOrCancelledException(operationCanceledException, originalCancellationToken);
            }
            catch (SocketException socketException)
            {
                if (socketException.InnerException?.GetType().IsAssignableFrom(typeof(OperationCanceledException)) ==
                    true)
                {
                    throw TimeoutOrCancelledException(socketException.InnerException, originalCancellationToken);
                }

                throw;
            }
            finally
            {
                cancellationTokenWithTimeout.Dispose();
            }
        }

        private Exception TimeoutOrCancelledException(Exception exception, CancellationToken originalCancellationToken)
        {
            if (originalCancellationToken.IsCancellationRequested)
            {
                throw exception;
            }

            throw new RedisOperationTimeoutException(this);
        }
    }
}