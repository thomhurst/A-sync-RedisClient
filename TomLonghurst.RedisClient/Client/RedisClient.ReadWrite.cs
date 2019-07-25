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

        private long _outStandingOperations;

        public long OutstandingOperations => Interlocked.Read(ref _outStandingOperations);

        private long _operationsPerformed;

        private IDuplexPipe _pipe;
        private ReadResult _readResult;

        internal string LastAction;

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> SendAndReceiveAsync<T>(string command,
            Func<ValueTask<T>> responseReader,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            LastAction = "Throwing Cancelled Exception due to Cancelled Token";
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);


            if (!isReconnectionAttempt)
            {
                LastAction = "Waiting for Send/Receive lock to be free";
                await _sendSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            Log.Debug($"Executing Command: {command}");
            LastCommand = command;

            Interlocked.Increment(ref _operationsPerformed);

            try
            {
                if (!isReconnectionAttempt)
                {
                    await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                }

                await Write(command);

                LastAction = "Reading Bytes Async";
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
                if (!isReconnectionAttempt)
                {
                    LastAction = "Releasing Send/Receive Lock";
                    _sendSemaphoreSlim.Release();
                }
            }
        }

        private ValueTask<FlushResult> Write(string command)
        {
            LastAction = "Writing Bytes";
            var pipeWriter = _pipe.Output;
            var writeSpan = pipeWriter.GetSpan(32);

            var byteCount = command.AsUtf8BytesSpan(out var bytesSpan);

            bytesSpan.CopyTo(writeSpan);

            pipeWriter.Advance(byteCount);

            return _pipe.Output.FlushAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<Memory<byte>> ReadData(bool readToEnd = false)
        {
            var buffer = _readResult.Buffer;

            if (buffer.IsEmpty && _readResult.IsCompleted)
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var line = await GetLine();
            
            var firstChar = line.First();

            if (string.IsNullOrEmpty(line))
            {
                throw new Exception("No data to process");
            }

            if (firstChar == '-')
            {
                throw new RedisFailedCommandException(line, LastCommand);
            }

            if (firstChar == '$')
            {
                if (line == "$-1")
                {
                    return null;
                }
                
                LastAction = "Reading Data Synchronously in ReadData";
                if (!_pipe.Input.TryRead(out _readResult))
                {
                    LastAction = "Reading Data Asynchronously in ReadData";
                    _readResult = await _pipe.Input.ReadAsync().ConfigureAwait(false);
                }
                
                buffer = _readResult.Buffer;

                if (long.TryParse(line.Substring(1), out var byteSizeOfData))
                {
                    var bytes = new byte[byteSizeOfData].AsMemory();
                    var dataBuffer = buffer.Slice(0, Math.Min(byteSizeOfData, buffer.Length));
                    var bytesReceived = dataBuffer.Length;
                    var consumeThisBuffer = bytesReceived;
                    
                    dataBuffer.CopyTo(bytes.Slice(0, (int) bytesReceived).Span);

                    while (bytesReceived < byteSizeOfData)
                    {
                        LastAction = "Advancing Buffer in ReadData Loop";
                        var consumed = buffer.Slice(consumeThisBuffer);
                        if (consumed.Start.GetInteger() == buffer.End.GetInteger())
                        {
                            _pipe.Input.AdvanceTo(buffer.End);
                        }
                        else
                        {
                            _pipe.Input.AdvanceTo(consumed.Start, buffer.End);
                        }

                        LastAction = "Reading Data Synchronously in ReadData Loop";
                        if (!_pipe.Input.TryRead(out _readResult))
                        {
                            LastAction = "Reading Data Asynchronously in ReadData Loop";
                            _readResult = await _pipe.Input.ReadAsync().ConfigureAwait(false);
                        }
                        
                        buffer = _readResult.Buffer;
                        buffer.Slice(0, Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).CopyTo(bytes.Slice((int) bytesReceived, (int) Math.Min(buffer.Length, byteSizeOfData - bytesReceived)).Span);
                        bytesReceived += buffer.Length;
                        consumeThisBuffer = buffer.Length;
                    }

                    if (readToEnd)
                    {
                        LastAction = "Advancing Buffer to End of Line";
                        _pipe.Input.AdvanceTo(buffer.End);
                    }
                    else
                    {
                        LastAction = "Advancing Buffer to Line Terminator";
                        _readResult = await _pipe.Input.AdvanceToLineTerminator(_readResult);
                    }

                    return bytes;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }

        private async ValueTask<string> GetLine()
        {
            LastAction = "Reading until End of Line found";
            _readResult = await _pipe.Input.ReadUntilEndOfLineFound(_readResult);
            
            LastAction = "Finding End of Line Position";
            var endOfLineAfterByteCount = _readResult.Buffer.GetEndOfLinePosition();
            
            if (endOfLineAfterByteCount == null)
            {
                throw new Exception("Can't find EOL");
            }
            
            var buffer = _readResult.Buffer;
            
            buffer = buffer.Slice(0, endOfLineAfterByteCount.Value);

            var line = buffer.Slice(0, buffer.Length - 2).AsString();

            LastAction = "Advancing Buffer to End of Line";
            _pipe.Input.AdvanceTo(endOfLineAfterByteCount.Value);
            
            return line;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<string> ReadLine()
        {
            return await GetLine();
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