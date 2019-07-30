using System;
using System.IO.Pipelines;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        private RedisClient()
        {
            CreatePipeOptions();

            CreateCommandClasses();
        }

        private void CreateCommandClasses()
        {
            _clusterCommands = new ClusterCommands(this);
            _serverCommands = new ServerCommands(this);
        }

        private void CreatePipeOptions()
        {
            Options = new Lazy<RedisPipeOptions>(() =>
            {
                const int defaultMinimumSegmentSize = 1024 * 8;

                const long sendPauseWriterThreshold = 512 * 1024;
                const long sendResumeWriterThreshold = sendPauseWriterThreshold / 2;

                const long receivePauseWriterThreshold = 128 * 1024 * 1024;
                const long receiveResumeWriterThreshold = receivePauseWriterThreshold / 2;

                var pipeScheduler = PipeScheduler.ThreadPool;
                var defaultPipeOptions = PipeOptions.Default;

                var receivePipeOptions = new PipeOptions(
                    defaultPipeOptions.Pool,
                    pipeScheduler,
                    pipeScheduler,
                    receivePauseWriterThreshold,
                    receiveResumeWriterThreshold,
                    defaultMinimumSegmentSize,
                    false);

                var sendPipeOptions = new PipeOptions(
                    defaultPipeOptions.Pool,
                    pipeScheduler,
                    pipeScheduler,
                    sendPauseWriterThreshold,
                    sendResumeWriterThreshold,
                    defaultMinimumSegmentSize,
                    false);

                return new RedisPipeOptions
                {
                    SendOptions = sendPipeOptions,
                    ReceiveOptions = receivePipeOptions
                };
            });
        }
    }
}