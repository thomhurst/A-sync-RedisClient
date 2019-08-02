using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using RedisClientTest;
using TomLonghurst.RedisClient.Client;
using TomLonghurst.RedisClient.Enums;

namespace TestConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new RedisClientConfig(TestInformation.Host,
                TestInformation.Port,
                TestInformation.Password) {Ssl = true};
            
            var redisManager = new RedisClientManager(config, 1);
            
            var tomLonghurstRedisClient = redisManager.GetRedisClient();
            
            await tomLonghurstRedisClient.StringSetAsync("SingleKey",
                "123",
                120,
                AwaitOptions.AwaitCompletion);

            var tasks = new List<Task>();

            for (var taskCount = 0; taskCount < 50; taskCount++)
            {
                var task = Task.Run(async () =>
                {
                    for (var outer = 0; outer < 5; outer++)
                    {
                        var tomLonghurstRedisClientStopwatch = Stopwatch.StartNew();

                        for (var i = 0; i < 200; i++)
                        {
                            var redisValue = await tomLonghurstRedisClient.StringGetAsync("SingleKey");
                        }

                        tomLonghurstRedisClientStopwatch.Stop();
                        var tomLonghurstRedisClientStopwatchTimeTaken =
                            tomLonghurstRedisClientStopwatch.ElapsedMilliseconds;
                        Console.WriteLine($"Time Taken: {tomLonghurstRedisClientStopwatchTimeTaken} ms");
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}