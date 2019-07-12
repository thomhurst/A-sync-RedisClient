using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Helpers;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        private static readonly Logger Log = new Logger();

        private readonly SemaphoreSlim _sendAndReceiveSemaphoreSlim = new SemaphoreSlim(1, 1);

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
                await _sendAndReceiveSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            Interlocked.Increment(ref _operationsPerformed);

            try
            {
                if (!isReconnectionAttempt)
                {
                    await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                }

                Write(bytes);

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
                    _sendAndReceiveSemaphoreSlim.Release();
                }
                
                RecreatePipe();
            }
        }

        private void RecreatePipe()
        {
            if (_redisClientConfig.Ssl)
            {
                pipe = StreamConnection.GetDuplex(_sslStream, SendPipeOptions, ReceivePipeOptions);
            }
            else
            {
                pipe = SocketConnection.Create(_socket, SendPipeOptions, ReceivePipeOptions);
            }
        }

        private void Write(byte[] bytes)
        {
            pipe.Output.Write(bytes);

            WriteLineEnd();
        }

        private void WriteLineEnd()
        {
            var span = pipe.Output.GetSpan(2);
            span[0] = (byte) '\r';
            span[1] = (byte) '\n';
            pipe.Output.Advance(2);
            // pipe.Output.Complete();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<object> ExpectSuccess()
        {
            var response = await ReadLineAsync();
            if (response.StartsWith("-"))
            {
                throw new RedisFailedCommandException(response);
            }
            
            // pipe.Input.Complete();

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

                if (int.TryParse(line.Substring(1), out var byteSizeOfData))
                {
                    var byteBuffer = new byte [byteSizeOfData];

                    var bytesRead = 0;
                    do
                    {
                        var read = _bufferedStream.Read(byteBuffer, bytesRead, byteSizeOfData - bytesRead);

                        if (read < 1)
                        {
                            throw new UnexpectedRedisResponseException(
                                $"Invalid termination mid stream: {byteBuffer.FromUtf8()}");
                        }

                        bytesRead += read;
                    } while (bytesRead < byteSizeOfData);

                    if (_bufferedStream.ReadByte() != '\r' || _bufferedStream.ReadByte() != '\n')
                    {
                        throw new UnexpectedRedisResponseException($"Invalid termination: {byteBuffer.FromUtf8()}");
                    }

                    return byteBuffer;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }

        private async Task<string> ReadLineAsync()
        {

            var stringBuilder = new StringBuilder();

            var reader = pipe.Input;

//            while (true)
//            {

            var result = await reader.ReadAsync();

            var buffer = result.Buffer;
            SequencePosition? position;

            do
            {
                // Look for a EOL in the buffer
                position = buffer.PositionOf((byte) '\n');

                if (position != null)
                {
                    // Process the line
                    ProcessLine(buffer.Slice(0, position.Value), stringBuilder);
                    buffer = buffer.Slice(buffer.GetPosition(0, position.Value));
                }
            } while (position == null);

            // Tell the PipeReader how much of the buffer we have consumed
            reader.AdvanceTo(buffer.Start, buffer.End);
//            }

            return stringBuilder.ToString();
        }

        private void ProcessLine(ReadOnlySequence<byte> buffer, StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }

            if (buffer.IsSingleSegment)
            {
                stringBuilder.Append(Encoding.UTF8.GetString(buffer.First.Span));
            }
            else
            {
                stringBuilder.Append(
                    string.Create((int) buffer.Length, buffer, (span, sequence) =>
                    {
                        foreach (var segment in sequence)
                        {
                            Encoding.UTF8.GetChars(segment.Span, span);

                            span = span.Slice(segment.Length);
                        }
                    })
                );
            }
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