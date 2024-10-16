using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace TomLonghurst.AsyncRedisClient.Pipes;

public class SocketPipe : IDuplexPipe
{
    public static SocketPipe GetDuplexPipe(Socket? socket, PipeOptions sendPipeOptions,
        PipeOptions receivePipeOptions) =>
        new(socket, sendPipeOptions, receivePipeOptions, true, true);

    private readonly Socket? _innerSocket;

    private readonly Pipe? _readPipe;
    private readonly Pipe? _writePipe;

    public void Reset()
    {
        _writePipe?.Reset();
        _readPipe?.Reset();
    }

    private SocketPipe(Socket? socket, PipeOptions? sendPipeOptions, PipeOptions? receivePipeOptions, bool read,
        bool write)
    {
        ArgumentNullException.ThrowIfNull(socket);

        sendPipeOptions ??= PipeOptions.Default;

        receivePipeOptions ??= PipeOptions.Default;

        _innerSocket = socket;

        if (!(read || write))
        {
            throw new ArgumentException("At least one of read/write must be set");
        }

        if (read)
        {
            _readPipe = new Pipe(receivePipeOptions);
                
            receivePipeOptions.ReaderScheduler.Schedule(o => _ = CopyFromSocketToReadPipe(), null);
        }
            
        if (write)
        {
            _writePipe = new Pipe(sendPipeOptions);
                
            sendPipeOptions.WriterScheduler.Schedule(o => _ = CopyFromWritePipeToSocket(), null);
        }
    }

    public PipeWriter Output =>
        _writePipe?.Writer ?? throw new InvalidOperationException("Cannot write to this pipe");

    public PipeReader Input =>
        _readPipe?.Reader ?? throw new InvalidOperationException("Cannot read from this pipe");

    private async Task CopyFromSocketToReadPipe()
    {
        Exception? exception = null;
        var writer = _readPipe!.Writer;

        try
        {
            while (true)
            {
                try
                {
                    var memory = writer.GetMemory(512);

                    var bytesRead = await _innerSocket!.ReceiveAsync(memory, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    writer.Advance(bytesRead);

                    var result = await writer.FlushAsync();

                    if (result.IsCompleted || result.IsCanceled)
                    {
                        break;
                    }
                }
                catch (IOException)
                {
                    // TODO Why does this occur?
                    //    "Unable to read data from the transport connection: The I/O operation has been aborted because of either a thread exit or an application request."
                }
            }
        }
        catch (Exception e)
        {
            exception = e;
        }

        await writer.CompleteAsync(exception);
    }

    private async Task CopyFromWritePipeToSocket()
    {
        Exception? exception = null;
        var reader = _writePipe!.Reader;

        try
        {
            while (true)
            {
                var pendingReadResult = reader.ReadAsync();

                var readResult = await pendingReadResult;

                do
                {
                    if (!readResult.Buffer.IsEmpty)
                    {
                        if (readResult.Buffer.IsSingleSegment)
                        {
                            var writeTask = WriteSingle(readResult.Buffer);
                            if (!writeTask.IsCompleted)
                            {
                                await writeTask;
                            }
                        }
                        else
                        {
                            var writeTask = WriteMultiple(readResult.Buffer);
                            if (!writeTask.IsCompleted)
                            {
                                await writeTask;
                            }
                        }
                    }

                    reader.AdvanceTo(readResult.Buffer.End);

                } while (!(readResult.Buffer.IsEmpty && readResult.IsCompleted)
                         && reader.TryRead(out readResult));

                if ((readResult.IsCompleted || readResult.IsCanceled) && readResult.Buffer.IsEmpty)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            exception = e;
        }
        finally
        {
            await reader.CompleteAsync(exception);
        }
    }

    private Task WriteSingle(in ReadOnlySequence<byte> buffer)
    {
        var valueTask = _innerSocket!.SendAsync(buffer.First, SocketFlags.None);
        return valueTask.IsCompletedSuccessfully ? Task.CompletedTask : valueTask.AsTask();
    }

    private async Task WriteMultiple(ReadOnlySequence<byte> buffer)
    {
        foreach (var segment in buffer)
        {
            await _innerSocket!.SendAsync(segment, SocketFlags.None);
        }
    }
}