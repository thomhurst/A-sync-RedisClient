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

        public static IDuplexPipe GetDuplexPipe(Stream stream, PipeOptions sendPipeOptions,
            PipeOptions receivePipeOptions)
            => new StreamPipe(stream, sendPipeOptions, receivePipeOptions, true, true);

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

        public PipeWriter Output =>
            _writePipe?.Writer ?? throw new InvalidOperationException("Cannot write to this pipe");

        public PipeReader Input =>
            _readPipe?.Reader ?? throw new InvalidOperationException("Cannot read from this pipe");

        private void CopyFromStreamToReadPipe()
        {
            Exception exception = null;
            var writer = _readPipe.Writer;

            Task.Run(new Action(async delegate
            {
                try
                {
                    while (true)
                    {
                        var memory = writer.GetMemory();
#if NETCORE
                        var bytesRead = await _innerStream.ReadAsync(memory).ConfigureAwait(false);
#else
                        var arr = memory.GetArraySegment();

                        var bytesRead = await _innerStream.ReadAsync(arr.Array, arr.Offset, arr.Count)
                            .ConfigureAwait(false);
#endif

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        writer.Advance(bytesRead);

                        var result = await writer.FlushAsync().ConfigureAwait(false);
                        if (result.IsCompleted)
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
            }));
        }

        private long _totalBytesSent, _totalBytesReceived;

        //long IMeasuredDuplexPipe.TotalBytesSent => Interlocked.Read(ref _totalBytesSent);
        //long IMeasuredDuplexPipe.TotalBytesReceived => Interlocked.Read(ref _totalBytesReceived);

        private void CopyFromWritePipeToStream()
        {
            Exception exception = null;
            var reader = _writePipe.Reader;
            
            Task.Run(new Action(async delegate
            {
                try
                {
                    while (true)
                    {
                        var pendingReadResult = reader.ReadAsync();
                        
                        if (!pendingReadResult.IsCompleted)
                        {
                            await _innerStream.FlushAsync().ConfigureAwait(false);
                        }

                        var readResult = await pendingReadResult.ConfigureAwait(false);
                        
                        if (!readResult.Buffer.IsEmpty)
                        {
                            foreach (var segment in readResult.Buffer)
                            {
#if NETCORE
                            await _innerStream.WriteAsync(segment);
#else
                                var arraySegment = segment.GetArraySegment();
                                await _innerStream
                                    .WriteAsync(arraySegment.Array, arraySegment.Offset, arraySegment.Count)
                                    .ConfigureAwait(false);
#endif
                            }

                            await _innerStream.FlushAsync().ConfigureAwait(false);
                        }

                        reader.AdvanceTo(readResult.Buffer.End);

                        if (readResult.IsCompleted && readResult.Buffer.IsEmpty)
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                }

                reader.Complete(exception);
            }));
        }
    }
}