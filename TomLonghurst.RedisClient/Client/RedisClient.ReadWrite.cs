using System;
using System.Collections.Generic;
using System.Linq;
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
            CancellationToken cancellationToken)
        {
            Log.Debug($"Executing Command: {command}");

            return SendAndReceiveAsync(command.ToUtf8Bytes(), responseReader, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> SendAndReceiveAsync<T>(byte[] bytes,
            Func<T> responseReader,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await TryConnectAsync(cancellationToken).ConfigureAwait(false);

            Interlocked.Increment(ref _outStandingOperations);

            await _sendSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

            Interlocked.Increment(ref _operationsPerformed);

            try
            {
                await TryConnectAsync(cancellationToken).ConfigureAwait(false);

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
            catch (SocketException innerException)
            {
                IsConnected = false;
                throw new RedisConnectionException(innerException);
            }
            catch (NotSupportedException innerException)
            {
                IsConnected = false;
                throw new RedisConnectionException(innerException);
            }
            finally
            {
                Interlocked.Decrement(ref _outStandingOperations);
                _sendSemaphoreSlim.Release();
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
        private string ExpectData()
        {
            return ReadData().FromUtf8();
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
        private IEnumerable<RedisValue<string>> ExpectArray()
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

            var results = new byte [count][];
            for (var i = 0; i < count; i++)
            {
                results[i] = ReadData();
            }

            return results.ToRedisValues();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] ReadData()
        {
            var line = ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                throw new UnexpectedRedisResponseException("Zero Length Response from Redis");
            }

            var firstChar = line.First();

            if (firstChar == '-')
            {
                throw new RedisFailedCommandException(line);
            }

            if (firstChar == '$')
            {
                if (line == "$-1")
                {
                    return null;
                }

                if (int.TryParse (line.Substring(1), out var byteSizeOfData)){
                    var byteBuffer = new byte [byteSizeOfData];

                    var bytesRead = 0;
                    do {
                        var read = _bufferedStream.Read(byteBuffer, bytesRead, byteSizeOfData - bytesRead);

                        if (read < 1)
                        {
                            throw new UnexpectedRedisResponseException($"Invalid termination mid stream: {byteBuffer.FromUtf8()}");
                        }

                        bytesRead += read; 
                    }
                    while (bytesRead < byteSizeOfData);

                    if (_bufferedStream.ReadByte() != '\r' || _bufferedStream.ReadByte() != '\n')
                    {
                        throw new UnexpectedRedisResponseException($"Invalid termination: {byteBuffer.FromUtf8()}");
                    }

                    return byteBuffer;
                }
                
                throw new UnexpectedRedisResponseException("Invalid length");
            }
            
            throw new UnexpectedRedisResponseException ($"Unexpected reply: {line}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadLine()
        {
            var stringBuilder = new StringBuilder ();
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

            return stringBuilder.ToString ();
        }

        private async Task TryConnectAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
            {
                return;
            }
            
            try
            {
                await Task.Run(async () => await ConnectAsync(), cancellationToken);
            }
            catch (Exception innerException)
            {
                IsConnected = false;
                throw new RedisConnectionException(innerException);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<T> RunWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action, CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();

            try
            {
                var cancellationTokenWithTimeout =
                    CancellationTokenHelper.CancellationTokenWithTimeout(_redisClientConfig.Timeout,
                        originalCancellationToken);
                return await action.Invoke(cancellationTokenWithTimeout);
            }
            catch (OperationCanceledException)
            {
                if (originalCancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                throw new RedisOperationTimeoutException(this);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask RunWithTimeout(Func<CancellationToken, ValueTask> action, CancellationToken originalCancellationToken)
        {
            originalCancellationToken.ThrowIfCancellationRequested();

            try
            {
                var cancellationTokenWithTimeout =
                    CancellationTokenHelper.CancellationTokenWithTimeout(_redisClientConfig.Timeout,
                        originalCancellationToken);
                await action.Invoke(cancellationTokenWithTimeout);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                CheckTimeout(operationCanceledException, originalCancellationToken);
            }
            catch (SocketException socketException)
            {
                if (socketException.InnerException?.GetType().IsAssignableFrom(typeof(OperationCanceledException)) == true)
                {
                    CheckTimeout(socketException.InnerException, originalCancellationToken);
                    return;
                }

                throw;
            }
        }

        private void CheckTimeout(Exception exception, CancellationToken originalCancellationToken)
        {
            if (originalCancellationToken.IsCancellationRequested)
            {
                throw exception;
            }

            throw new RedisOperationTimeoutException(this);
        }
    }
}