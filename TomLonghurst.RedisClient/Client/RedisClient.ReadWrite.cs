using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nerdbank.Streams;
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
        private IDuplexPipe _sslPipe;

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

                if (_redisClientConfig.Ssl)
                {
                    Write(command, _sslPipe.Output);
                    //_sslStream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    _socket.Send(command.ToUtf8Bytes());
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
        


        public static int MaxInt32TextLen => 11;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteLineTerminator(PipeWriter writer)
        {
            var span = writer.GetSpan(2);
            span[0] = (byte)'\r';
            span[1] = (byte)'\n';
            writer.Advance(2);
        }
        
        ValueTask<bool> Write(string message, PipeWriter writer)
        {
            // (this may be multiple GetSpan/Advance calls, or a loop,
            // depending on what makes sense for the message/protocol)
            var byteCount = Encoding.UTF8.GetByteCount(message);
            var span = writer.GetSpan(byteCount + 2);
            Encoding.UTF8.GetBytes(message.AsSpan(), span);
            
            writer.Write(span);
            //writer.Advance(byteCount);
            WriteLineTerminator(writer);

            return FlushAsync(writer);
        }
        
        private async ValueTask<string> GetNextMessage()
        {
            var reader = _sslPipe.Input;
            while (true)
            {
                var read = await reader.ReadAsync();
                if (read.IsCanceled)
                {
                    //ThrowCanceled();
                    throw new TaskCanceledException();
                }

                // can we find a complete frame?
                var buffer = read.Buffer;
                if (TryParseFrame(
                    buffer,
                    out string nextMessage,
                    out SequencePosition consumedTo))
                {
                    reader.AdvanceTo(consumedTo);
                    return nextMessage;
                }
                reader.AdvanceTo(buffer.Start, buffer.End);
                if (read.IsCompleted)
                {
                    //ThrowEOF();
                    throw new EndOfStreamException();
                }        
            }
        }
        
        private static bool TryParseFrame(
            ReadOnlySequence<byte> buffer,
            out string nextMessage,
            out SequencePosition consumedTo)
        {
            // find the end-of-line marker
            var eol = buffer.PositionOf((byte)'\n');
            if (eol == null)
            {
                nextMessage = default;
                consumedTo = default;
                return false;
            }

            // read past the line-ending
            consumedTo = buffer.GetPosition(1, eol.Value);
            // consume the data
            var payload = buffer.Slice(0, eol.Value);
            nextMessage = ReadSomeMessageType(payload);
            return true;
        }

        private static string ReadSomeMessageType(in ReadOnlySequence<byte> payload)
        {
            return payload.GetString();
        }

        private static async ValueTask<bool> FlushAsync(PipeWriter writer)
        {
            // apply back-pressure etc
            var flush = await writer.FlushAsync();
            // tell the calling code whether any more messages
            // should be written
            return !(flush.IsCanceled || flush.IsCompleted);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<object> ExpectSuccess()
        {
            var response = await GetNextMessage();
            if (response.StartsWith("-"))
            {
                throw new RedisFailedCommandException(response);
            }

            return new object();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ExpectData()
        {
            return (await ReadData()).ToArray().FromUtf8();
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

            var results = new Memory<byte> [count];
            for (var i = 0; i < count; i++)
            {
                results[i] = await ReadData();
            }

            return results.Select(result => result.Span.ToArray()).ToRedisValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<Memory<byte>> ReadData()
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
                    var byteBuffer = new byte [byteSizeOfData].AsMemory();

                    var pipeReader = _sslStream.UsePipeReader();
                    SequencePosition? indexOfTerminator;
                    var bytesRead = 0L;
                    do
                    {
                        var readResult = await pipeReader.ReadAsync();

                        if (readResult.Buffer.Length < 1)
                        {
                            throw new UnexpectedRedisResponseException(
                                $"Invalid termination mid stream: {byteBuffer.ToArray().FromUtf8()}");
                        }
                        
                        readResult.Buffer.CopyTo(byteBuffer.Span);
                        
                        indexOfTerminator = readResult.Buffer.PositionOf(1, (byte) '\n');
                        
                        pipeReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);

                        bytesRead += readResult.Buffer.Length;
                    } while (bytesRead < byteSizeOfData);

                    if (indexOfTerminator == null)
                    {
                        throw new UnexpectedRedisResponseException($"Invalid termination: {byteBuffer.ToArray().FromUtf8()}");
                    }
                    
                    pipeReader.AdvanceTo(indexOfTerminator.Value);

                    return byteBuffer;
                }

                throw new UnexpectedRedisResponseException("Invalid length");
            }

            throw new UnexpectedRedisResponseException($"Unexpected reply: {line}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ReadLine()
        {
            var stringBuilder = new StringBuilder();
            SequencePosition? indexOfTerminator;
            int c;

            var reader = _sslStream.UsePipeReader();

            var readResult = await reader.ReadAsync();

            var result = readResult.Buffer.ToArray().FromUtf8();
            
            reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
            
            indexOfTerminator = readResult.Buffer.PositionOf(1, (byte) '\n');
            
            if (indexOfTerminator == null)
            {
                throw new UnexpectedRedisResponseException($"Invalid termination: {result}");
            }
                    
            reader.AdvanceTo(indexOfTerminator.Value);
            
            return result;
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