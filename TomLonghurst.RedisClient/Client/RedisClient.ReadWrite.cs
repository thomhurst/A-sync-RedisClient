﻿using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nerdbank.Streams;
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

        private PipeReader _pipeReader;
        private ReadResult _readResult;

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> SendAndReceiveAsync<T>(string command,
            Func<T> responseReader,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);


            if (!isReconnectionAttempt)
            {
                await _sendSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            
            Log.Debug($"Executing Command: {command}");
            _lastCommand = command;
            var encodedCommand = command.ToRedisProtocol();
            var bytes = encodedCommand.ToUtf8Bytes();

            Interlocked.Increment(ref _operationsPerformed);

            try
            {
                if (!isReconnectionAttempt)
                {
                    await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                }

                if (_redisClientConfig.Ssl)
                {
                    _sslStream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    _socket.Send(bytes);
                }

                T invokedResponse;
                try
                {
                    _pipeReader = _sslStream.UsePipeReader();
                    _readResult = await _pipeReader.ReadAsync();
                    invokedResponse = responseReader.Invoke();
                }
                finally
                {
                    _pipeReader.Complete();   
                }

                return invokedResponse;
            }
            catch (Exception innerException)
            {
                if (innerException.IsSameOrSubclassOf(typeof(RedisException)) || innerException.IsSameOrSubclassOf(typeof(OperationCanceledException)))
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
                    _sendSemaphoreSlim.Release();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] ReadData()
        {
            var buffer = _readResult.Buffer;
            var bufferReader = new BufferReader(buffer);

            if (buffer.IsEmpty && _readResult.IsCompleted)
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var firstChar = (char) bufferReader.PeekByte();

            if (firstChar == '-')
            {
                throw new RedisFailedCommandException("TODO", _lastCommand);
            }
            
            var crlfOffsetFromCurrent = BufferReader.FindNextCrLf(bufferReader);

            var line = bufferReader.ConsumeAsBuffer(crlfOffsetFromCurrent).AsString();

            if (firstChar == '$')
            {
                if (line == "$-1")
                {
                    return null;
                }

                if (int.TryParse(line.Substring(1), out var byteSizeOfData))
                {
                    var data = bufferReader.ConsumeAsBuffer(byteSizeOfData).ToArray();

                    var dataEndOfLine = BufferReader.FindNextCrLf(bufferReader);
                    
                    bufferReader.Consume(dataEndOfLine);

                    return data;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadLine()
        {
            var buffer = _readResult.Buffer;
            var bufferReader = new BufferReader(buffer);

            var endOfLine = BufferReader.FindNextCrLf(bufferReader);

            var payload = bufferReader.ConsumeAsBuffer(endOfLine).AsString();

            return payload;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> RunWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action,
            CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();
            var cancellationTokenWithTimeout =
                CancellationTokenHelper.CancellationTokenWithTimeout(_redisClientConfig.Timeout,
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
                CancellationTokenHelper.CancellationTokenWithTimeout(_redisClientConfig.Timeout,
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