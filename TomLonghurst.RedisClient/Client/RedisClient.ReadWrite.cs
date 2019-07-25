using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Helpers;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static readonly Logger Log = new Logger();

        private readonly SemaphoreSlim _sendSemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _reconnectSemaphoreSlim = new SemaphoreSlim(1, 1);

        private long _outStandingOperations;

        public long OutstandingOperations => Interlocked.Read(ref _outStandingOperations);

        private long _operationsPerformed;

        private IDuplexPipe _pipe;
        private ReadResult _readResult;

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> SendAndReceiveAsync<T>(string command,
            Func<Task<T>> responseReader,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);


            if (isReconnectionAttempt)
            {
                await _reconnectSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _sendSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            Log.Debug($"Executing Command: {command}");
            _lastCommand = command;

            Interlocked.Increment(ref _operationsPerformed);

            try
            {
                if (!isReconnectionAttempt)
                {
                    await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                }

                await Write(command);

                _readResult = await _pipe.Input.ReadAsync().ConfigureAwait(false);

                return await responseReader.Invoke();
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
                if (isReconnectionAttempt)
                {
                    _reconnectSemaphoreSlim.Release();
                }
                else
                {
                    _sendSemaphoreSlim.Release();
                }
            }
        }

        private async Task Write(string command)
        {
            var flushResult =  await _pipe.Output.WriteAsync(command.ToRedisProtocol().ToUtf8Bytes().AsMemory()).ConfigureAwait(false);;
            if (!flushResult.IsCompleted)
            {
                await _pipe.Output.FlushAsync().ConfigureAwait(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<Memory<byte>> ReadData(bool readToEnd = false)
        {
            var buffer = _readResult.Buffer;

            if (buffer.IsEmpty && _readResult.IsCompleted)
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var line = await GetLine(buffer);
            
            var firstChar = line.First();

            if (string.IsNullOrEmpty(line))
            {
                throw new Exception("No data to process");
            }

            if (firstChar == '-')
            {
                throw new RedisFailedCommandException(line, _lastCommand);
            }

            if (firstChar == '$')
            {
                if (line == "$-1")
                {
                    return null;
                }
                
                if (!_pipe.Input.TryRead(out _readResult) || _readResult.Buffer.IsEmpty)
                {
                    _readResult = await _pipe.Input.ReadAsync();
                }
                
                buffer = _readResult.Buffer;

                if (long.TryParse(line.Substring(1), out var byteSizeOfData))
                {
                    var bytes = new byte[byteSizeOfData].AsMemory();
                    var dataBuffer = buffer.Slice(0, Math.Min(byteSizeOfData, buffer.Length));
                    var bytesReceived = dataBuffer.Length;
                    
                    dataBuffer.CopyTo(bytes.Slice(0, (int) bytesReceived).Span);

                    while (bytesReceived < byteSizeOfData)
                    {
                        _pipe.Input.AdvanceTo(buffer.End);
                        
                        if (!_pipe.Input.TryRead(out _readResult) || _readResult.Buffer.IsEmpty)
                        {
                            _readResult = await _pipe.Input.ReadAsync();
                        }
                        
                        buffer = _readResult.Buffer;
                        buffer.Slice(0, Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).CopyTo(bytes.Slice((int) bytesReceived, (int) Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).Span);
                        bytesReceived += buffer.Length;
                    }

                    if (readToEnd)
                    {
                        _pipe.Input.AdvanceTo(buffer.End);
                    }
                    else
                    {
                        await _pipe.Input.AdvanceToLineTerminator(_readResult);
                    }

                    return bytes;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }

        private async Task<string> GetLine(ReadOnlySequence<byte> buffer)
        {
            var endOfLineAfterByteCount = await _pipe.Input.ReadUntilEndOfLineFound(_readResult);

            if (endOfLineAfterByteCount == null)
            {
                throw new Exception("Can't find EOL");
            }
            
            buffer = buffer.Slice(0, endOfLineAfterByteCount.Value);
            
            var line = buffer.Slice(0, buffer.Length - 2).AsString();

            _pipe.Input.AdvanceTo(endOfLineAfterByteCount.Value);
            
            return line;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ReadLine()
        {
            var buffer = _readResult.Buffer;
            return await GetLine(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            finally
            {
                cancellationTokenWithTimeout.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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