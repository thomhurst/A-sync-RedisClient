﻿using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using TomLonghurst.AsyncRedisClient.Exceptions;
using TomLonghurst.AsyncRedisClient.Helpers;
using TomLonghurst.AsyncRedisClient.Models.Backlog;
using TomLonghurst.AsyncRedisClient.Extensions;
using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;
using TomLonghurst.AsyncRedisClient.Pipes;
#if !NETSTANDARD2_0
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

        
        internal ValueTask<T> SendOrQueueAsync<T>(string command,
            AbstractResultProcessor<T> abstractResultProcessor,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            LastUsed = DateTime.Now;
            
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

        private ValueTask<T> QueueToBacklog<T>(string command, AbstractResultProcessor<T> abstractResultProcessor,
            CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            _backlog.Enqueue(new BacklogItem<T>(command, cancellationToken, taskCompletionSource, abstractResultProcessor, this, _pipeReader));

            cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
            
            return new ValueTask<T>(taskCompletionSource.Task);
        }

        internal async ValueTask<T> SendAndReceiveAsync<T>(string command, AbstractResultProcessor<T> abstractResultProcessor,
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

        private ValueTask<FlushResult> Write(string command)
        {
            return Write(RedisCommandConverter.ConvertSingleCommand(command));
        }
        
        private ValueTask<FlushResult> Write(ReadOnlyMemory<byte> bytes)
        {
            _written++;
            return _pipeWriter.WriteAsync(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> RunWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action,
            CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();
            
            var cancellationTokenWithTimeout = CancellationTokenHelper.CancellationTokenWithTimeout(ClientConfig.Timeout, originalCancellationToken);

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