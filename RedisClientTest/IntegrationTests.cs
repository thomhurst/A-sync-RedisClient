using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TomLonghurst.RedisClient.Client;
using TomLonghurst.RedisClient.Enums;

namespace RedisClientTest
{
    public class Tests
    {
        private RedisClientManager _redisManager;
        private RedisClientConfig _config;
        private RedisClient _client;

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
            _client = await _redisManager.GetRedisClientAsync();
        }

        [Test]
        public async Task Test1Async()
        {
            var sw = Stopwatch.StartNew();
            
            var ping = await _client.Ping();
            
            var getDoesntExist = await _client.StringGetAsync(new [] { "Blah1", "Blah2" });
            
            await _client.StringSetAsync("TestyMcTestFace", "123", 120, AwaitOptions.FireAndForget);
            await _client.StringSetAsync("TestyMcTestFace2", "1234", 120, AwaitOptions.FireAndForget);
            await _client.StringSetAsync("TestyMcTestFace3", "1235", 120, AwaitOptions.FireAndForget);
            
            var getValue = await _client.StringGetAsync(new [] { "TestyMcTestFace", "TestyMcTestFace2", "TestyMcTestFace3" });
            var getValueSingle = await _client.StringGetAsync("TestyMcTestFace");

            var keyExistsFalse = await _client.KeyExistsAsync("KeyExistsAsync");
            var keyExistsTrue = await _client.KeyExistsAsync("TestyMcTestFace");
            
            var timeTaken = sw.ElapsedMilliseconds;
            Console.WriteLine($"Time Taken: {timeTaken} ms");
        }
        
        [Test]
        public async Task Ping()
        {
            var pong = await _client.Ping();
            Assert.AreEqual(true, pong.IsSuccessful);
            Assert.AreEqual("PONG", pong.Message);
        }
        
        [Test]
        public async Task GetNonExistingKey()
        {
            var nonExistingKey = await _client.StringGetAsync("Blah1");
            Assert.That(nonExistingKey.HasValue, Is.False);
        }
        
        [Test]
        public async Task GetNonExistingKeys()
        {
            var nonExistingKeys = await _client.StringGetAsync(new [] { "Blah1", "Blah2" });
            Assert.That(nonExistingKeys, Is.Empty);
        }
        
        [Test]
        public async Task GetExistingKeyAmongstNonExistingKeys()
        {
            await _client.StringSetAsync("Exists", "123", 30, AwaitOptions.AwaitCompletion);
            var nonExistingKeys = await _client.StringGetAsync(new [] { "Blah1", "Blah2", "Exists", "Blah4", "Blah5" });
            Assert.That(nonExistingKeys.Count(), Is.EqualTo(1));
        }
        
        [Test]
        public async Task SetGetDeleteSingleKey()
        {
            await _client.StringSetAsync("SingleKey", "123", AwaitOptions.AwaitCompletion);
            var redisValue = await _client.StringGetAsync("SingleKey");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
            
            await _client.DeleteKeyAsync("SingleKey", AwaitOptions.AwaitCompletion);
            redisValue = await _client.StringGetAsync("SingleKey");
            Assert.That(redisValue.IsNull, Is.EqualTo(true));
        }

        [Test]
        public async Task SetGetSingleKeyWithTtl()
        {
            await _client.StringSetAsync("SingleKeyWithTtl", "123", 30, AwaitOptions.AwaitCompletion);
            var redisValue = await _client.StringGetAsync("SingleKeyWithTtl");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
        }

        [Test]
        public async Task SetGetMultipleKey()
        {
            var keyValues = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("MultiKey1", "1"),
                new KeyValuePair<string, string>("MultiKey2", "2"),
                new KeyValuePair<string, string>("MultiKey3", "3")
            };
            
            await _client.StringSetAsync(keyValues, AwaitOptions.AwaitCompletion);
            var redisValues = await _client.StringGetAsync(new [] { "MultiKey1", "MultiKey1", "MultiKey3" });
            Assert.That(redisValues.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task KeyExists()
        {
            await _client.StringSetAsync("KeyExistsCheck", "123", 30, AwaitOptions.AwaitCompletion);
            var exists = await _client.KeyExistsAsync("KeyExistsCheck");
            var doesntExist = await _client.KeyExistsAsync("KeyDoesntExistsCheck");
            
            Assert.That(exists, Is.EqualTo(true));
            Assert.That(doesntExist, Is.EqualTo(false));
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