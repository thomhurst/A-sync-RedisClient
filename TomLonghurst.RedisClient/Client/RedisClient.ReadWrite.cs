using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Helpers;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Models.Backlog;
using TomLonghurst.RedisClient.Models.Commands;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static readonly Logger Log = new Logger();

        internal virtual bool CanQueueToBacklog { get; set; } = true;
        
        private SemaphoreSlim _sendAndReceiveSemaphoreSlim = new SemaphoreSlim(1, 1);
        
        private long _outStandingOperations;

        public long OutstandingOperations => Interlocked.Read(ref _outStandingOperations);

        private long _operationsPerformed;

        private IDuplexPipe _pipe;
        private ReadResult _readResult;

        public object IsBusyLock = new object();
        public bool IsBusy;

        internal string LastAction;

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        public DateTime LastUsed { get; internal set; }
        
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTask<T> SendOrQueueAsync<T>(IRedisCommand command,
            IResultProcessor<T> resultProcessor,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            LastUsed = DateTime.Now;
            
            LastAction = "Throwing Cancelled Exception due to Cancelled Token";
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);
            
            if (!isReconnectionAttempt && CanQueueToBacklog && IsBusy)
            {
                return QueueToBacklog(command, resultProcessor, cancellationToken);
            }

            return SendAndReceiveAsync(command, resultProcessor, cancellationToken, isReconnectionAttempt);
        }

        private ValueTask<T> QueueToBacklog<T>(IRedisCommand command, IResultProcessor<T> resultProcessor,
            CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            _backlog.Enqueue(new BacklogItem<T>(command, cancellationToken, taskCompletionSource, resultProcessor, this, _pipe));

            return new ValueTask<T>(taskCompletionSource.Task);
        }

        internal async ValueTask<T> SendAndReceiveAsync<T>(IRedisCommand command, IResultProcessor<T> resultProcessor,
            CancellationToken cancellationToken, bool isReconnectionAttempt)
        {
            IsBusy = true;
            
            Log.Debug($"Executing Command: {command}");
            LastCommand = command;

            Interlocked.Increment(ref _operationsPerformed);

            try
            {
                if (!isReconnectionAttempt)
                {
                    await _sendAndReceiveSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                    
                    if (!IsConnected)
                    {
                        await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                await Write(command);

                LastAction = "Reading Bytes Async";

                return await resultProcessor.Start(this, _pipe);
            }
            catch (Exception innerException)
            {
                if (innerException.IsSameOrSubclassOf(typeof(RedisException)) ||
                    innerException.IsSameOrSubclassOf(typeof(OperationCanceledException)))
                {
                    throw;
                }

                DisposeNetwork();
                IsConnected = false;
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

        internal ValueTask Write(IRedisCommand command)
        {
            var encodedCommandList = command.EncodedCommandList;

            LastAction = "Writing Bytes";
            var pipeWriter = _pipe.Output;

#if NETCORE
            foreach (var encodedCommand in encodedCommandList)
            {
                pipeWriter.Write(encodedCommand.AsSpan());
//                var bytesSpan = pipeWriter.GetSpan(encodedCommand.Length);
//                encodedCommand.CopyTo(bytesSpan);
//                pipeWriter.Advance(encodedCommand.Length);
            }

            var task = pipeWriter.FlushAsync();
            if (!task.IsCompleted)
            {
                return WriteSlowAsync(task);
            }
#else
            var task = pipeWriter.WriteAsync(encodedCommandList.SelectMany(x => x).ToArray().AsMemory());

            if (!task.IsCompleted)
            {
                return WriteSlowAsync(task);
            }
#endif

            return default;
            
            async ValueTask WriteSlowAsync(ValueTask<FlushResult> flushTask)
            {
                var flushResult = await flushTask;

                // Cancellation can be triggered by PipeWriter.CancelPendingFlush
                if (flushResult.IsCanceled)
                {
                    throw new OperationCanceledException();
                }
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> RunWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action,
            CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();
            
            var cancellationTokenWithTimeout =
                CancellationTokenHelper.CancellationTokenWithTimeout(ClientConfig.Timeout,
                    originalCancellationToken);

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
                cancellationTokenWithTimeout.Dispose();
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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