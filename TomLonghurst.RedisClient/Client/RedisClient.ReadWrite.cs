using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial.Arenas;
using TomLonghurst.RedisClient.Exceptions;
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
        
        private readonly Arena<RawResult> _arena = new Arena<RawResult>(ArenaOptions.Default);

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
                await FlushWriteAsync();

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

        private async Task FlushWriteAsync()
        {
            await pipe.Output.FlushAsync().ConfigureAwait(false);
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
            return (await ReadRaw()).FirstOrDefault().GetString();
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

        private async Task<List<RawResult>> ReadRaw()
        {
            var results = new List<RawResult>();
            
            while (true)
            {
                var pipeReader = pipe.Input;
                if (pipeReader == null)
                {
                    return null;
                }
                
                if (!pipeReader.TryRead(out var result))
                {
                    result = await pipeReader.ReadAsync().ConfigureAwait(false);
                }

                var buffer = result.Buffer;
                int handledBytes = 0;
                if (!buffer.IsEmpty)
                {
                    handledBytes = Process(ref buffer, out results);
                }

                pipeReader.AdvanceTo(buffer.Start, buffer.End);

                Console.WriteLine("PIPE RESULT: " + result.Buffer);
                
                if (result.IsCompleted)
                {
                    pipeReader.Complete();
                    break;
                }
            }

            return results;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> ReadLine()
        {
            
            return (await ReadRaw()).First().GetString();
        }

        private int Process(ref ReadOnlySequence<byte> buffer, out List<RawResult> results)
        {
            int messageCount = 0;
            
            results = new List<RawResult>();

            while (!buffer.IsEmpty)
            {
                var reader = new ByteSpanReader(buffer);
                var result = ParseResult(ref reader, in buffer);
                results.Add(result);
                try
                {
                    if (result.HasValue)
                    {
                        buffer = reader.SliceFromCurrent();

                        messageCount++;
                        //MatchResult(result);
                    }
                    else
                    {
                        break; // remaining buffer isn't enough; give up
                    }
                }
                finally
                {
                    _arena.Reset();
                }
            }
            return messageCount;
        }

        private RawResult ParseResult(ref ByteSpanReader reader, in ReadOnlySequence<byte> buffer)
        {
            var prefix = reader.PeekByte();
            
            if (prefix < 0)
            {
                return RawResult.Nil;
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
                    return ReadBulkString(ref reader);
                case '*': // array
                    reader.Consume(1);
                    return ReadArray(ref reader, in buffer);
// TODO
                //                default:
//                    // string s = Format.GetString(buffer);
//                    if (allowInlineProtocol)
//                    {
//                        return ParseInlineProtocol(arena, ReadLineTerminatedString(ResultType.SimpleString, ref reader));
//                    }
//                    throw new InvalidOperationException("Unexpected response prefix: " + (char)prefix);
            }
            
            throw new InvalidOperationException("Unexpected response prefix: " + (char)prefix);
        }

        
        private RawResult ReadLineTerminatedString(ResultType type, ref ByteSpanReader reader)
        {
            int crlfOffsetFromCurrent = ByteSpanReader.FindNextCrLf(reader);
            if (crlfOffsetFromCurrent < 0)
            {
                return RawResult.Nil;
            }

            var payload = reader.ConsumeAsBuffer(crlfOffsetFromCurrent);
            reader.Consume(2);

            return new RawResult(type, payload, false);
        }
        
        private RawResult ReadBulkString(ref ByteSpanReader reader)
        {
            var prefix = ReadLineTerminatedString(ResultType.Integer, ref reader);
            if (prefix.HasValue)
            {
                if (!prefix.TryGetInt64(out long i64))
                {
                    // TODO
                    // throw ExceptionFactory.ConnectionFailure(includeDetailInExceptions, ConnectionFailureType.ProtocolFailure, "Invalid bulk string length", server);
                }
                
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
                            throw new Exception();
                            // TODO
                            // throw ExceptionFactory.ConnectionFailure(includeDetailInExceptions, ConnectionFailureType.ProtocolFailure, "Invalid bulk string terminator", server);
                    }
                }
            }
            return RawResult.Nil;
        }
        
        private RawResult ReadArray(ref ByteSpanReader reader, in ReadOnlySequence<byte> buffer)
        {
            var itemCount = ReadLineTerminatedString(ResultType.Integer, ref reader);
            if (itemCount.HasValue)
            {
                if (!itemCount.TryGetInt64(out long i64))
                {
                    // TODO
                    throw new Exception();
                    //throw ExceptionFactory.ConnectionFailure(includeDetailInExceptions, ConnectionFailureType.ProtocolFailure, "Invalid array length", server);
                }
                
                int itemCountActual = checked((int)i64);

                if (itemCountActual < 0)
                {
                    //for null response by command like EXEC, RESP array: *-1\r\n
                    return RawResult.NullMultiBulk;
                }
                else if (itemCountActual == 0)
                {
                    //for zero array response by command like SCAN, Resp array: *0\r\n
                    return RawResult.EmptyMultiBulk;
                }

                var oversized = _arena.Allocate(itemCountActual);
                var result = new RawResult(oversized, false);

                if (oversized.IsSingleSegment)
                {
                    var span = oversized.FirstSpan;
                    for(int i = 0; i < span.Length; i++)
                    {
                        if (!(span[i] = ParseResult(ref reader, in buffer)).HasValue)
                        {
                            return RawResult.Nil;
                        }
                    }
                }
                else
                {
                    foreach(var span in oversized.Spans)
                    {
                        for (int i = 0; i < span.Length; i++)
                        {
                            if (!(span[i] = ParseResult(ref reader, in buffer)).HasValue)
                            {
                                return RawResult.Nil;
                            }
                        }
                    }
                }
                return result;
            }
            return RawResult.Nil;
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