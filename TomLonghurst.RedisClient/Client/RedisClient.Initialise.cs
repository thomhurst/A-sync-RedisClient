using System;
using System.IO.Pipelines;
using System.Threading;
using TomLonghurst.RedisClient.Enums;
using TomLonghurst.RedisClient.Models;
using TomLonghurst.RedisClient.Pipes;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {
        private readonly ClientType _clientType;
        private static PipeThreadPoolScheduler _pipeScheduler;

        static RedisClient()
        {
            CreatePipeOptions();
        }
        
        protected RedisClient(ClientType clientType)
        {
            _clientType = clientType;
            _weakReference = new WeakReference<RedisClient>(this);
            CreateCommandClasses();
        }

        private void CreateCommandClasses()
        {
            _clusterCommands = new ClusterCommands(this);
            _serverCommands = new ServerCommands(this);
        }

        private static void CreatePipeOptions()
        {
            Options = new Lazy<RedisPipeOptions>(() =>
            {
                const int defaultMinimumSegmentSize = 1024 * 8;

                const long sendPauseWriterThreshold = 512 * 1024;
                const long sendResumeWriterThreshold = sendPauseWriterThreshold / 2;

                const long receivePauseWriterThreshold = 1024 * 1024 * 1024;
                const long receiveResumeWriterThreshold = receivePauseWriterThreshold / 4 * 3;

                _pipeScheduler = PipeThreadPoolScheduler.Default;
                var defaultPipeOptions = PipeOptions.Default;

                var receivePipeOptions = new PipeOptions(
                    defaultPipeOptions.Pool,
                    _pipeScheduler,
                    _pipeScheduler,
                    receivePauseWriterThreshold,
                    receiveResumeWriterThreshold,
                    defaultMinimumSegmentSize,
                    false);

                var sendPipeOptions = new PipeOptions(
                    defaultPipeOptions.Pool,
                    _pipeScheduler,
                    _pipeScheduler,
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