using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Helpers;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static readonly Logger Log = new Logger();

        private readonly SemaphoreSlim _sendSemaphoreSlim = new SemaphoreSlim(1, 1);

        private long _outStandingOperations;

        public long OutstandingOperations => Interlocked.Read(ref _outStandingOperations);

        private long _operationsPerformed;

        public long OperationsPerformed => Interlocked.Read(ref _operationsPerformed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<T> SendAndReceiveAsync<T>(string command,
            Func<T> responseReader,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            Log.Debug($"Executing Command: {command}");

            return SendAndReceiveAsync(command.ToUtf8Bytes(), responseReader, cancellationToken, isReconnectionAttempt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> SendAndReceiveAsync<T>(byte[] bytes,
            Func<T> responseReader,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Interlocked.Increment(ref _outStandingOperations);


            if (!isReconnectionAttempt)
            {
                await _sendSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

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

                return responseReader.Invoke();
            }
            catch (RedisConnectionException)
            {
                throw;
            }
            catch (Exception innerException)
            {
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
        private object ExpectSuccess()
        {
            var response = ReadLine();
            if (response.StartsWith("-"))
            {
                throw new RedisFailedCommandException(response);
            }

            return new object();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ExpectData()
        {
#if NETCORE
            return (await ReadData()).FromUtf8();
#elif NETSTANDARD
            return ReadData().FromUtf8();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ExpectWord()
        {
            var word = ReadLine();

            if (!word.StartsWith("+"))
            {
                throw new UnexpectedRedisResponseException(word);
            }

            return word.Substring(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExpectNumber()
        {
            var line = ReadLine();

            if (!line.StartsWith(":") || !int.TryParse(line.Substring(1), out var number))
            {
                throw new UnexpectedRedisResponseException(line);
            }

            return number;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<IEnumerable<RedisValue<string>>> ExpectArray()
        {
            var arrayWithCountLine = ReadLine();

            if (!arrayWithCountLine.StartsWith("*"))
            {
                throw new UnexpectedRedisResponseException(arrayWithCountLine);
            }

            if (!int.TryParse(arrayWithCountLine.Substring(1), out var count))
            {
                throw new UnexpectedRedisResponseException("Error getting message count");
            }

            var results = new string [count];
            for (var i = 0; i < count; i++)
            {
#if NETCORE
                results[i] = (await ReadData()).FromUtf8();
#elif NETSTANDARD
                results[i] = ReadData().FromUtf8();
#endif
            }

            return results.ToRedisValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadLine()
        {
            var stringBuilder = new StringBuilder();
            int c;

            while ((c = _bufferedStream.ReadByte()) != -1)
            {
                if (c == '\r')
                {
                    continue;
                }

                if (c == '\n')
                {
                    break;
                }

                stringBuilder.Append((char) c);
            }

            return stringBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> RunWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action,
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