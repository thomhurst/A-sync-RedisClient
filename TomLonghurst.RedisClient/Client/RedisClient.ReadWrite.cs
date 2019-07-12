using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
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
        private async ValueTask<T> SendAndReceiveAsync<T>(string command,
            Func<T> responseReader,
            CancellationToken cancellationToken,
            bool isReconnectionAttempt = false)
        {
            Log.Debug($"Executing Command: {command}");

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

                Write(command);

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

        private unsafe void Write(string value)
        {
            fixed (char* charPointer = value)
            {
                var expectedLength = Encoding.UTF8.GetByteCount(value);
                var span = pipe.Output.GetSpan(expectedLength);
                
                fixed (byte* bytePointer = span)
                {
                    Encoding.UTF8.GetBytes(charPointer, value.Length, bytePointer, expectedLength);
                }
                pipe.Output.Advance(expectedLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<object> ExpectSuccess()
        {
            var response = await ReadLine();
            if (response.StartsWith("-"))
            {
                throw new RedisFailedCommandException(response);
            }

            return new object();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ExpectData()
        {
            return (await ReadData()).FromUtf8();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ExpectWord()
        {
            var word = await ReadLine();

            if (!word.StartsWith("+"))
            {
                throw new UnexpectedRedisResponseException(word);
            }

            return word.Substring(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<int> ExpectNumber()
        {
            var line = await ReadLine();

            if (!line.StartsWith(":") || !int.TryParse(line.Substring(1), out var number))
            {
                throw new UnexpectedRedisResponseException(line);
            }

            return number;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<IEnumerable<RedisValue<string>>> ExpectArray()
        {
            var arrayWithCountLine = await ReadLine();

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
                results[i] = await ReadData();
            }

            return results.ToRedisValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<byte[]> ReadData()
        {
            var line = await ReadLine();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ReadLine()
        {
            var pipeReader = pipe.Input;
            if (pipeReader == null)
            {
                return null;
            }

            while (true)
            {
                if (!pipeReader.TryRead(out var result))
                {
                    result = await pipeReader.ReadAsync().ConfigureAwait(false);
                }

                var buffer = result.Buffer;
                int handledBytes = 0;
                if (!buffer.IsEmpty)
                {
                    handledBytes = Process(ref buffer);
                }

                pipeReader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            Console.WriteLine("PIPE RESULT: " + result.Buffer.IsEmpty);
            
            //span.IndexOf('\n')-1
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

        private string ParseResult(ByteSpanReader reader)
        {
            var prefix = reader.PeekByte();
            if (prefix < 0)
            {
                return string.Empty;
            }
            switch (prefix)
            {
                case '+': // simple string
                    reader.Consume(1);
                    return ReadLineTerminatedString(ResultType.SimpleString, ref reader);
                case '-': // error
                    reader.Consume(1);
                    return ReadLineTerminatedString(ResultType.Error, ref reader);
                case ':': // integer
                    reader.Consume(1);
                    return ReadLineTerminatedString(ResultType.Integer, ref reader);
                case '$': // bulk string
                    reader.Consume(1);
                    return ReadBulkString(ref reader, includeDetilInExceptions, server);
                case '*': // array
                    reader.Consume(1);
                    return ReadArray(arena, in buffer, ref reader, includeDetilInExceptions, server);
                default:
                    // string s = Format.GetString(buffer);
                    if (allowInlineProtocol) return ParseInlineProtocol(arena, ReadLineTerminatedString(ResultType.SimpleString, ref reader));
                    throw new InvalidOperationException("Unexpected response prefix: " + (char)prefix);
            }
        }

        
        private static string ReadLineTerminatedString(ByteSpanReader reader)
        {
            int crlfOffsetFromCurrent = ByteSpanReader.FindNextCrLf(reader);
            if (crlfOffsetFromCurrent < 0)
            {
                return null;
            }

            var payload = reader.ConsumeAsBuffer(crlfOffsetFromCurrent);
            reader.Consume(2);

            return new RawResult(type, payload, false);
        }
        
        private static RawResult ReadBulkString(ref ByteSpanReader reader, bool includeDetailInExceptions, ServerEndPoint server)
        {
            var prefix = ReadLineTerminatedString(ResultType.Integer, ref reader);
            if (prefix.HasValue)
            {
                if (!prefix.TryGetInt64(out long i64)) throw ExceptionFactory.ConnectionFailure(includeDetailInExceptions, ConnectionFailureType.ProtocolFailure, "Invalid bulk string length", server);
                int bodySize = checked((int)i64);
                if (bodySize < 0)
                {
                    return new RawResult(ResultType.BulkString, ReadOnlySequence<byte>.Empty, true);
                }

                if (reader.TryConsumeAsBuffer(bodySize, out var payload))
                {
                    switch (reader.TryConsumeCRLF())
                    {
                        case ConsumeResult.NeedMoreData:
                            break; // see NilResult below
                        case ConsumeResult.Success:
                            return new RawResult(ResultType.BulkString, payload, false);
                        default:
                            throw ExceptionFactory.ConnectionFailure(includeDetailInExceptions, ConnectionFailureType.ProtocolFailure, "Invalid bulk string terminator", server);
                    }
                }
            }
            return RawResult.Nil;
        }
        
        private int Process(ref ReadOnlySequence<byte> buffer)
        {
            return -1;
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