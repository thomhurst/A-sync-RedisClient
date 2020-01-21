using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using RedisClientTest;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Compression;
using TomLonghurst.AsyncRedisClient.Enums;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TestConsoleApp
{
    class Program
    {
        private static RedisClientManager _redisManager;
        private static Task<RedisClient> TomLonghurstRedisClient => _redisManager.GetRedisClientAsync();
        
        private static readonly List<KeyValuePair<string, string>> TestData = new List<KeyValuePair<string, string>>();
        private static readonly Dictionary<int, DateTime> _lastActive = new Dictionary<int, DateTime>();


        static async Task Main(string[] args)
        {

            var currentProcessId = Process.GetCurrentProcess().Id;

            for (int i = 0; i < 10000; i++)
            {
                TestData.Add(new KeyValuePair<string, string>(CreateString(20), CreateString(50000)));
            }

            var runForDuration = TimeSpan.FromMinutes(5);

            var start = DateTime.Now;

            var config = new RedisClientConfig(TestInformation.Host,
                TestInformation.Port,
                TestInformation.Password) {Ssl = true};

            _redisManager = new RedisClientManager(config, 1);

            var tasks = new List<Task>();

            for (var taskCount = 0; taskCount < 150; taskCount++)
            {
                var taskId = taskCount;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        while (DateTime.Now - start < runForDuration)
                        {
                            var tomLonghurstRedisClientStopwatch = Stopwatch.StartNew();

                            await DoSomething();

                            tomLonghurstRedisClientStopwatch.Stop();
                            var tomLonghurstRedisClientStopwatchTimeTaken =
                                tomLonghurstRedisClientStopwatch.ElapsedMilliseconds;
                            Console.WriteLine(
                                $"PID {currentProcessId} -- Task {taskId} -- Time Taken: {tomLonghurstRedisClientStopwatchTimeTaken} ms");
                            _lastActive[taskId] = DateTime.Now;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception occured on task {taskId}");
                        Console.WriteLine(e);
                        throw;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            foreach (var key in _lastActive.Keys)
            {
                var dateTime = _lastActive[key];
                if (DateTime.Now - dateTime > TimeSpan.FromSeconds(10))
                {
                    Console.WriteLine($"Task {key} was last active at {dateTime.ToLongTimeString()} - {(DateTime.Now - dateTime).TotalMilliseconds} ms ago");
                }
            }
            
            Console.WriteLine($"Finished at {DateTime.Now.ToLongTimeString()}");
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        static async Task DoSomething()
        {
            if (Random.Next(0, 2) != 0)
            {
                await (await _redisManager.GetRedisClientAsync()).StringGetAsync(TestData.PickRandom().Key, CompressionType.None);
            }
            else
            {
                var (key, value) = TestData.PickRandom();
                
                await (await _redisManager.GetRedisClientAsync()).StringSetAsync(key, value, 120, CompressionType.None,
                    AwaitOptions.FireAndForget);
            }
        } 
        
        private static readonly Random Random = new Random();
        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[Random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}