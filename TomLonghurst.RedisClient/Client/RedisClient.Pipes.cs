using System;
using System.IO.Pipelines;
using System.Threading;
using Pipelines.Sockets.Unofficial;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        private string _name;
        private DedicatedThreadPoolPipeScheduler _schedulerPool;
        private PipeOptions SendPipeOptions, ReceivePipeOptions;

        private const int DEFAULT_WORKERS = 10, MINIMUM_SEGMENT_SIZE = 8 * 1024;

        public void InitialisePipes()
        {
            _name = GetType().Name;

            const long
                Receive_PauseWriterThreshold =
                    4L * 1024 * 1024 * 1024; // receive: let's give it up to 4GiB of buffer for now
            const long Receive_ResumeWriterThreshold = 3L * 1024 * 1024 * 1024; // (large replies get crazy big)

            var defaultPipeOptions = PipeOptions.Default;

            var Send_PauseWriterThreshold = Math.Max(
                512 * 1024, // send: let's give it up to 0.5MiB
                defaultPipeOptions.PauseWriterThreshold); // or the default, whichever is bigger
            var Send_ResumeWriterThreshold = Math.Max(
                Send_PauseWriterThreshold / 2,
                defaultPipeOptions.ResumeWriterThreshold);

            _schedulerPool = new DedicatedThreadPoolPipeScheduler(_name + ":IO",
                workerCount: DEFAULT_WORKERS,
                priority: ThreadPriority.AboveNormal);
            SendPipeOptions = new PipeOptions(
                pool: defaultPipeOptions.Pool,
                readerScheduler: _schedulerPool,
                writerScheduler: _schedulerPool,
                pauseWriterThreshold: Send_PauseWriterThreshold,
                resumeWriterThreshold: Send_ResumeWriterThreshold,
                minimumSegmentSize: Math.Max(defaultPipeOptions.MinimumSegmentSize, MINIMUM_SEGMENT_SIZE),
                useSynchronizationContext: false);
            ReceivePipeOptions = new PipeOptions(
                pool: defaultPipeOptions.Pool,
                readerScheduler: _schedulerPool,
                writerScheduler: _schedulerPool,
                pauseWriterThreshold: Receive_PauseWriterThreshold,
                resumeWriterThreshold: Receive_ResumeWriterThreshold,
                minimumSegmentSize: Math.Max(defaultPipeOptions.MinimumSegmentSize, MINIMUM_SEGMENT_SIZE),
                useSynchronizationContext: false);
        }
    }
}