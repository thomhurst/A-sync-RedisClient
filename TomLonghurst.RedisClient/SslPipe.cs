using System;
using System.IO.Pipelines;
using System.Net.Security;
using System.Threading.Tasks;

namespace TomLonghurst.RedisClient
{
    public class SslPipe : IDuplexPipe
    {
        private readonly SslStream _sslStream;
        private readonly Pipe _readPipe = new Pipe();
        private readonly Pipe _writePipe = new Pipe();
        private readonly PipeScheduler _pipeScheduler = PipeScheduler.ThreadPool;

        public SslPipe(SslStream sslStream)
        {
            _sslStream = sslStream ?? throw new ArgumentNullException();
        }

        public PipeReader Input => _readPipe.Reader;
        public PipeWriter Output => _writePipe.Writer;

        public async Task CopyFromStreamToReadPipe()
        {
            Exception error = null;
            try
            {
                while (true)
                {
                    // note we'll usually get *much* more than we ask for
                    var buffer = _readPipe.Writer.GetMemory(1);

                    int bytes = await _sslStream.ReadAsync(buffer);

                    _readPipe.Writer.Advance(bytes);

                    if (bytes == 0)
                    {
                        break; // source EOF
                    }

                    var flush = await _readPipe.Writer.FlushAsync();
                    if (flush.IsCompleted || flush.IsCanceled)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                _readPipe.Writer.Complete(error);
            }
        }

        public async Task CopyFromWritePipeToStream()
        {
            Exception error = null;
            try
            {
                while (true)
                {
                    var read = await _writePipe.Reader.ReadAsync();
                    var buffer = read.Buffer;
                    //if (buffer.IsCanceled) break;
                    if (buffer.IsEmpty && read.IsCompleted)
                    {
                        break;
                    }

                    // write everything we got to the stream
                    foreach (var segment in buffer)
                    {
                        await _sslStream.WriteAsync(segment);
                    }

                    _writePipe.Reader.AdvanceTo(buffer.End);
                    await _sslStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                _writePipe.Reader.Complete(error);
            }
        }
    }
}