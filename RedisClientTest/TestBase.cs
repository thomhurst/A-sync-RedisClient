using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using TomLonghurst.AsyncRedisClient.Client;

namespace RedisClientTest
{
    public class TestBase
    {
        protected RedisClientManager _redisManager;
        protected RedisClientConfig _config;
        protected Task<RedisClient> TomLonghurstRedisClient => _redisManager.GetRedisClientAsync();
        
        protected readonly string _largeValueJson = File.ReadAllText("large_json.json");

        // To test, create a Test Information static class to store your Host, Port and Password for your Test Redis Server
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new RedisClientConfig(TestInformation.Host,
                TestInformation.Port,
                TestInformation.Password) {Ssl = true};
            _redisManager = new RedisClientManager(_config, 1);
            _redisManager.GetAllRedisClients();
        }

        [SetUp]
        public void BeforeTest()
        {
            Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
        }
    }
}