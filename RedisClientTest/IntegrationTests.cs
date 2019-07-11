using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using TomLonghurst.RedisClient.Client;
using TomLonghurst.RedisClient.Enums;

namespace RedisClientTest
{
    public class Tests
    {
        private RedisClientManager _redisManager;
        private RedisClientConfig _config;
        private RedisClient _tomLonghurstRedisClient;

        // To test, create a Test Information static class to store your Host, Port and Password for your Test Redis Server
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new RedisClientConfig(TestInformation.Host, TestInformation.Port,
                TestInformation.Password)
            {
                Ssl = true
            };
            _redisManager = new RedisClientManager(_config, 1);
        }

        [SetUp]
        public async Task Setup()
        {
            _tomLonghurstRedisClient = await _redisManager.GetRedisClientAsync();
        }

        [Ignore("")]
        [Test]
        public async Task PerformanceTest()
        {
            var stackExchange = (await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
            {
                EndPoints = {{TestInformation.Host, TestInformation.Port}},
                Password = TestInformation.Password,
                Ssl = true
            })).GetDatabase(0);

            await stackExchange.StringSetAsync("SingleKey", "123");
            await _tomLonghurstRedisClient.StringSetAsync("SingleKey", "123", 120, AwaitOptions.AwaitCompletion);
            
            Console.WriteLine("StackExchange.Redis");
            
            var stackExchangeRedisClientStopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 100; i++)
            {
                //var redisValue = await stackExchange.StringGetAsync("SingleKey");
            }
            
            stackExchangeRedisClientStopwatch.Stop();
            var stackExchangeRedisClientStopwatchTimeTaken = stackExchangeRedisClientStopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Time Taken: {stackExchangeRedisClientStopwatchTimeTaken} ms");
            
            Console.WriteLine();
            Console.WriteLine();
            
            Console.WriteLine("TomLonghurst.RedisClient");
            
            var tomLonghurstRedisClientStopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 100; i++)
            {
                var redisValue = await _tomLonghurstRedisClient.StringGetAsync("SingleKey");
            }
            
            tomLonghurstRedisClientStopwatch.Stop();
            var tomLonghurstRedisClientStopwatchTimeTaken = tomLonghurstRedisClientStopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Time Taken: {tomLonghurstRedisClientStopwatchTimeTaken} ms");
        }

        [Test]
        public async Task Test1Async()
        {
            var sw = Stopwatch.StartNew();
            
            var pong = await _tomLonghurstRedisClient.Ping();
            Assert.AreEqual(true, pong.IsSuccessful);
            
            var getDoesntExist = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "Blah1", "Blah2" })).ToList();
            Assert.That(getDoesntExist.Count, Is.EqualTo(2));
            Assert.That(getDoesntExist.Count(value => value.HasValue), Is.EqualTo(0));
            
            await _tomLonghurstRedisClient.StringSetAsync("TestyMcTestFace", "123", 120, AwaitOptions.FireAndForget);
            await _tomLonghurstRedisClient.StringSetAsync("TestyMcTestFace2", "1234", 120, AwaitOptions.FireAndForget);
            await _tomLonghurstRedisClient.StringSetAsync("TestyMcTestFace3", "12345", 120, AwaitOptions.FireAndForget);
            
            var getValue = await _tomLonghurstRedisClient.StringGetAsync(new [] { "TestyMcTestFace", "TestyMcTestFace2", "TestyMcTestFace3" });
            Assert.That(getValue.Count(), Is.EqualTo(3));
            
            var getValueSingle = await _tomLonghurstRedisClient.StringGetAsync("TestyMcTestFace");
            Assert.That(getValueSingle.Value, Is.EqualTo("123"));

            
            await _tomLonghurstRedisClient.StringSetAsync("KeyExists", "123", AwaitOptions.AwaitCompletion);
            var keyExistsFalse = await _tomLonghurstRedisClient.KeyExistsAsync("KeyDoesntExist");
            Assert.That(keyExistsFalse, Is.EqualTo(false));
            var keyExistsTrue = await _tomLonghurstRedisClient.KeyExistsAsync("KeyExists");
            Assert.That(keyExistsTrue, Is.EqualTo(true));
            
            var timeTaken = sw.ElapsedMilliseconds;
            Console.WriteLine($"Time Taken: {timeTaken} ms");
        }
        
        [Test]
        public async Task Ping()
        {
            var pong = await _tomLonghurstRedisClient.Ping();
            Assert.AreEqual(true, pong.IsSuccessful);
            Assert.AreEqual("PONG", pong.Message);
        }
        
        [Test]
        public async Task GetNonExistingKey()
        {
            var nonExistingKey = await _tomLonghurstRedisClient.StringGetAsync("Blah1");
            Assert.That(nonExistingKey.HasValue, Is.False);
        }
        
        [Test]
        public async Task GetNonExistingKeys()
        {
            var nonExistingKeys = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "Blah1", "Blah2" })).ToList();
            Assert.That(nonExistingKeys.Count, Is.EqualTo(2));
            Assert.That(nonExistingKeys.Count(value => value.HasValue), Is.EqualTo(0));
        }
        
        [Test]
        public async Task GetExistingKeyAmongstNonExistingKeys()
        {
            await _tomLonghurstRedisClient.StringSetAsync("Exists", "123", 30, AwaitOptions.AwaitCompletion);
            var values = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "Blah1", "Blah2", "Exists", "Blah4", "Blah5" })).ToList();
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values.Count(value => value.HasValue), Is.EqualTo(1));
        }
        
        [Test]
        public async Task SetGetDeleteSingleKey()
        {
            await _tomLonghurstRedisClient.StringSetAsync("SingleKey", "123", AwaitOptions.AwaitCompletion);
            var redisValue = await _tomLonghurstRedisClient.StringGetAsync("SingleKey");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
            
            await _tomLonghurstRedisClient.DeleteKeyAsync("SingleKey", AwaitOptions.AwaitCompletion);
            redisValue = await _tomLonghurstRedisClient.StringGetAsync("SingleKey");
            Assert.That(redisValue.IsNull, Is.EqualTo(true));
        }

        [Test]
        public async Task SetGetSingleKeyWithTtl()
        {
            await _tomLonghurstRedisClient.StringSetAsync("SingleKeyWithTtl", "123", 30, AwaitOptions.AwaitCompletion);
            var redisValue = await _tomLonghurstRedisClient.StringGetAsync("SingleKeyWithTtl");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
        }

        [Test]
        public async Task SetGetMultipleKey()
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("MultiKey1", "1"),
                new KeyValuePair<string, string>("MultiKey2", "2"),
                new KeyValuePair<string, string>("MultiKey3", "3")
            };
            
            await _tomLonghurstRedisClient.StringSetAsync(keyValues, AwaitOptions.AwaitCompletion);
            var redisValues = await _tomLonghurstRedisClient.StringGetAsync(new [] { "MultiKey1", "MultiKey1", "MultiKey3" });
            Assert.That(redisValues.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task KeyExists()
        {
            await _tomLonghurstRedisClient.StringSetAsync("KeyExistsCheck", "123", 30, AwaitOptions.AwaitCompletion);
            var exists = await _tomLonghurstRedisClient.KeyExistsAsync("KeyExistsCheck");
            var doesntExist = await _tomLonghurstRedisClient.KeyExistsAsync("KeyDoesntExistsCheck");
            
            Assert.That(exists, Is.EqualTo(true));
            Assert.That(doesntExist, Is.EqualTo(false));
        }

        [Test, Ignore("No Cluster Support on Redis Server")]
        public async Task ClusterInfo()
        {
            var response = await _tomLonghurstRedisClient.ClusterInfo();
            var firstLine = response.Split("\n").First();
            Assert.That(firstLine, Is.EqualTo("cluster_state:ok"));
        }

        // Needs Access to Socket (which is private) to Close it
//        [Test]
//        public async Task Disconnected()
//        {
//            var client = await _redisManager.GetRedisClientAsync();
//            await client.StringSetAsync("DisconnectTest", "123", 120, AwaitOptions.FireAndForget);
//            var redisValue = await client.StringGetAsync("DisconnectTest");
//            Assert.AreEqual("123", redisValue.Value);
//            
//            client.Socket.Close();
//
//            try
//            {
//                await client.StringGetAsync("DisconnectTest");
//                Assert.Fail();
//            }
//            catch (SocketException e)
//            {
//            }
//
//            await Task.Delay(35000);
//
//            var redisValueAfterReconnect = await client.StringGetAsync("DisconnectTest");
//            Assert.AreEqual("123", redisValueAfterReconnect.Value);
//        }
    }
}