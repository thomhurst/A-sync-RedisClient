using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.AsyncRedisClient.Constants;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Helpers;
using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.Backlog;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;
using TomLonghurst.AsyncRedisClient.Pipes;
#if !NETSTANDARD2_0
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

        private string _lastAction;
        public static bool Debug;

        internal string LastAction
        {
            get => _lastAction ?? "Please set RedisClient.Debug to true to populate this field.";
            set
            {
#if DEBUG
                Console.WriteLine($"Last Action: {value}");
#endif
                if (Debug)
                {
                    _lastAction = value;
                }
            }
        }

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        public DateTime LastUsed { get; internal set; }

        private Func<RedisTelemetryResult, Task> _telemetryCallback;
        private int _written;
        private bool _isBusy;

        // TODO Make public
        private void SetTelemetryCallback(Func<RedisTelemetryResult, Task> telemetryCallback)
        {
            _telemetryCallback = telemetryCallback;
        }

        
        internal ValueTask<T> SendOrQueueAsync<T>(IRedisCommand command,
            AbstractResultProcessor<T> abstractResultProcessor,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            LastUsed = DateTime.Now;
            
#if DEBUG
            if (cancellationToken.IsCancellationRequested)
            {
                LastAction = LastActionConstants.ThrowingCancelledException;
            }
#endif
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);

//            return SendAndReceiveAsync(command, resultProcessor, cancellationToken, isReconnectionAttempt);
//            
            if (isReconnectionAttempt || _isBusy)
            {
                return SendAndReceiveAsync(command, abstractResultProcessor, cancellationToken, isReconnectionAttempt);
            }

            return QueueToBacklog(command, abstractResultProcessor, cancellationToken);
        }

        private ValueTask<T> QueueToBacklog<T>(IRedisCommand command, AbstractResultProcessor<T> abstractResultProcessor,
            CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            _backlog.Enqueue(new BacklogItem<T>(command, cancellationToken, taskCompletionSource, abstractResultProcessor, this, _pipeReader));

            cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
            
            return new ValueTask<T>(taskCompletionSource.Task);
        }

        internal async ValueTask<T> SendAndReceiveAsync<T>(IRedisCommand command, AbstractResultProcessor<T> abstractResultProcessor,
            CancellationToken cancellationToken, bool isReconnectionAttempt)
        {
            _isBusy = true;
            
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

                await Write(command).ConfigureAwait(false);

                return await abstractResultProcessor.Start(this, _pipeReader, cancellationToken);
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
                
                _isBusy = false;
            }
        }

        internal ValueTask<FlushResult> Write(IRedisCommand command)
        {
            _written++;
            var encodedCommandList = command.EncodedCommandList;

            LastAction = LastActionConstants.WritingBytes;

            return _pipeWriter.WriteAsync(encodedCommandList.SelectMany(x => x).ToArray());
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