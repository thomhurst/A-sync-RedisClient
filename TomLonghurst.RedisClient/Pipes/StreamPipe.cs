using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Extensions;

namespace TomLonghurst.RedisClient.Pipes
{
    public class StreamPipe : IDuplexPipe
    {
        
        public static IDuplexPipe GetDuplexPipe(Stream stream, PipeOptions sendPipeOptions, PipeOptions receivePipeOptions)
            =>  new StreamPipe(stream, sendPipeOptions, receivePipeOptions, true, true);
        
        private readonly Stream _innerStream;
        
        private readonly Pipe _readPipe;
        private readonly Pipe _writePipe;

        public StreamPipe(Stream stream, PipeOptions sendPipeOptions, PipeOptions receivePipeOptions, bool read,
            bool write)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (sendPipeOptions == null)
            {
                sendPipeOptions = PipeOptions.Default;
            }

            if (receivePipeOptions == null)
            {
                receivePipeOptions = PipeOptions.Default;
            }

            _innerStream = stream;
            
            if (!(read || write))
            {
                throw new ArgumentException("At least one of read/write must be set");
            }
            
            if (read)
            {
                if (!stream.CanRead)
                {
                    throw new InvalidOperationException("Cannot create a read pipe over a non-readable stream");
                }
                
                _readPipe = new Pipe(receivePipeOptions);
                
                receivePipeOptions.ReaderScheduler.Schedule(
                    obj => ((StreamPipe) obj).CopyFromStreamToReadPipe(), this);
            }

            if (write)
            {
                if (!stream.CanWrite)
                {
                    throw new InvalidOperationException("Cannot create a write pipe over a non-writable stream");
                }
                
                _writePipe = new Pipe(sendPipeOptions);
                
                sendPipeOptions.WriterScheduler.Schedule(
                    obj => ((StreamPipe) obj).CopyFromWritePipeToStream(), this);
            }
        }

        public PipeWriter Output => _readPipe?.Writer ?? throw new InvalidOperationException("Cannot write to this pipe");

        public PipeReader Input => _readPipe?.Reader ?? throw new InvalidOperationException("Cannot read from this pipe");

        private async Task CopyFromStreamToReadPipe()
        {
            Exception exception = null;
            var writer = _readPipe.Writer;
            try
            {
                while (true)
                {
                    var memory = writer.GetMemory(1);
#if NETCORE
                    var read = await _innerStream.ReadAsync(memory).ConfigureAwait(false);
#else
                    var arr = memory.GetArraySegment();

                    var read = await _innerStream.ReadAsync(arr.Array, arr.Offset, arr.Count).ConfigureAwait(false);
#endif
                    if (read <= 0)
                    {
                        break;
                    }
                    
                    writer.Advance(read);
                    Interlocked.Add(ref _totalBytesSent, read);
                    
                    var flush = await writer.FlushAsync().ConfigureAwait(false);
                    
                    if (flush.IsCompleted || flush.IsCanceled)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }

            writer.Complete(exception);
        }

        private long _totalBytesSent, _totalBytesReceived;

        //long IMeasuredDuplexPipe.TotalBytesSent => Interlocked.Read(ref _totalBytesSent);
        //long IMeasuredDuplexPipe.TotalBytesReceived => Interlocked.Read(ref _totalBytesReceived);

        private async Task CopyFromWritePipeToStream()
        {
            var reader = _writePipe.Reader;
            try
            {
                while (true)
                {
                    // ask to be awakened by work
                    var pending = reader.ReadAsync();

                    if (!pending.IsCompleted)
                    {
                        await _innerStream.FlushAsync().ConfigureAwait(false);
                    }

                    var result = await pending;
                    ReadOnlySequence<byte> buffer;
                    do
                    {
                        buffer = result.Buffer;

                        if (!buffer.IsEmpty)
                        {
                            await WriteBuffer(_innerStream, buffer).ConfigureAwait(false);
                            Interlocked.Add(ref _totalBytesReceived, buffer.Length);
                        }

                        reader.AdvanceTo(buffer.End);
                    } while (!(buffer.IsEmpty && result.IsCompleted) && reader.TryRead(out result));

                    if (result.IsCanceled)
                    {
                        break;
                    }

                    if (buffer.IsEmpty && result.IsCompleted)
                    {
                        break;
                    }
                }

                try
                {
                    reader.Complete(null);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                try
                {
                    reader.Complete(ex);
                }
                catch
                {
                }
            }
        }

        private static Task WriteBuffer(Stream target, in ReadOnlySequence<byte> data)
        {
            if (data.IsSingleSegment)
            {
#if NETCORE
                    var valueTask = target.WriteAsync(data.First);
                    return valueTask.IsCompletedSuccessfully ? Task.CompletedTask : valueTask.AsTask();
#else
                var arr = data.First.GetArraySegment();
                return target.WriteAsync(arr.Array, arr.Offset, arr.Count);
#endif
            }
            else
            {
                return WriteBufferAwaited(target, data);
            }
        }
        
        private static async Task WriteBufferAwaited(Stream ttarget, ReadOnlySequence<byte> ddata)
        {
            foreach (var segment in ddata)
            {
#if NETCORE
                await ttarget.WriteAsync(segment);
#else
                    var arr = segment.GetArraySegment();
                    await ttarget.WriteAsync(arr.Array, arr.Offset, arr.Count).ConfigureAwait(false);
#endif
            }
        }
    }
}