using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using TomLonghurst.RedisClient.Client;
using TomLonghurst.RedisClient.Enums;
using TomLonghurst.RedisClient.Models.RequestModels;

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
        
        [TestCase("value with a space")]
        [TestCase("value")]
        [TestCase("value with a\nnew line")]
        [TestCase("value with a\r\nnew line")]
        [Repeat(2)]
        public async Task Values(string value)
        {
            await _tomLonghurstRedisClient.StringSetAsync("key", value, AwaitOptions.AwaitCompletion);
            var redisValue = await _tomLonghurstRedisClient.StringGetAsync("key");
            
            Assert.That(redisValue.Value, Is.EqualTo(value));
        }
        
        [Test]
        [Repeat(2)]
        public async Task Multiple_Values_With_Space()
        {
            var data = new List<RedisKeyValue>
            {
                new RedisKeyValue("key1", "value with a space1"),
                new RedisKeyValue("key2", "value with a space2"),
                new RedisKeyValue("key3", "value with a space3")
            };
            
            await _tomLonghurstRedisClient.StringSetAsync(data, AwaitOptions.AwaitCompletion);
            
            var redisValue1 = await _tomLonghurstRedisClient.StringGetAsync("key1");
            Assert.That(redisValue1.Value, Is.EqualTo("value with a space1"));
            
            var redisValue2 = await _tomLonghurstRedisClient.StringGetAsync("key2");
            Assert.That(redisValue2.Value, Is.EqualTo("value with a space2"));
            
            var redisValue3 = await _tomLonghurstRedisClient.StringGetAsync("key3");
            Assert.That(redisValue3.Value, Is.EqualTo("value with a space3"));
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
                var redisValue = await stackExchange.StringGetAsync("SingleKey");
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
        [Repeat(2)]
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
        [Repeat(2)]
        public async Task Ping()
        {
            var pong = await _tomLonghurstRedisClient.Ping();
            Assert.AreEqual(true, pong.IsSuccessful);
            Assert.AreEqual("PONG", pong.Message);
        }
        
        [Test]
        [Repeat(2)]
        public async Task GetNonExistingKey()
        {
            var nonExistingKey = await _tomLonghurstRedisClient.StringGetAsync("Blah1");
            Assert.That(nonExistingKey.HasValue, Is.False);
        }
        
        [Test]
        [Repeat(2)]
        public async Task GetNonExistingKeys()
        {
            var nonExistingKeys = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "Blah1", "Blah2" })).ToList();
            Assert.That(nonExistingKeys.Count, Is.EqualTo(2));
            Assert.That(nonExistingKeys.Count(value => value.HasValue), Is.EqualTo(0));
        }
        
        [Test]
        [Repeat(2)]
        public async Task GetExistingKeyAmongstNonExistingKeys()
        {
            await _tomLonghurstRedisClient.StringSetAsync("Exists", "123", 30, AwaitOptions.AwaitCompletion);
            var values = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "Blah1", "Blah2", "Exists", "Blah4", "Blah5" })).ToList();
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values.Count(value => value.HasValue), Is.EqualTo(1));
        }
        
        [Test]
        [Repeat(2)]
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
        [Repeat(2)]
        public async Task SetGetSingleKeyWithTtl()
        {
            await _tomLonghurstRedisClient.StringSetAsync("SingleKeyWithTtl", "123", 30, AwaitOptions.AwaitCompletion);
            var redisValue = await _tomLonghurstRedisClient.StringGetAsync("SingleKeyWithTtl");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
        }

        [Test]
        [Repeat(2)]
        public async Task SetGetMultipleKey()
        {
            var keyValues = new List<RedisKeyValue>
            {
                new RedisKeyValue("MultiKey1", "1"),
                new RedisKeyValue("MultiKey2", "2"),
                new RedisKeyValue("MultiKey3", "3")
            };
            
            await _tomLonghurstRedisClient.StringSetAsync(keyValues, AwaitOptions.AwaitCompletion);
            var redisValues = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "MultiKey1", "MultiKey2", "MultiKey3" })).ToList();
            Assert.That(redisValues.Count(), Is.EqualTo(3));
            Assert.That(redisValues[0].Value, Is.EqualTo("1"));
            Assert.That(redisValues[1].Value, Is.EqualTo("2"));
            Assert.That(redisValues[2].Value, Is.EqualTo("3"));
        }

        [Test]
        [Repeat(2)]
        public async Task Pipelining_Multiple_Sets()
        {
            var keys = new[]
            {
                "Pipeline1", "Pipeline2", "Pipeline3", "Pipeline4", "Pipeline5", "Pipeline6", "Pipeline7", "Pipeline8"
            };
            var results = keys.Select(async key =>
                await _tomLonghurstRedisClient.StringSetAsync(key, "123", 30, AwaitOptions.FireAndForget));
            
            await Task.WhenAll(results);
        }
        
        [TestCase(AwaitOptions.AwaitCompletion)]
        [TestCase(AwaitOptions.FireAndForget)]
        [Repeat(2)]
        public async Task SetGetMultipleKeyWithTtl(AwaitOptions awaitOptions)
        {
            var keyValues = new List<RedisKeyValue>
            {
                new RedisKeyValue("MultiKeyWithTtl1", "1"),
                new RedisKeyValue("MultiKeyWithTtl2", "2"),
                new RedisKeyValue("MultiKeyWithTtl3", "3")
            };
            
            await _tomLonghurstRedisClient.StringSetAsync(keyValues, 120, awaitOptions);
            var redisValues = (await _tomLonghurstRedisClient.StringGetAsync(new [] { "MultiKeyWithTtl1", "MultiKeyWithTtl2", "MultiKeyWithTtl3" })).ToList();
            Assert.That(redisValues.Count, Is.EqualTo(3));
            Assert.That(redisValues[0].Value, Is.EqualTo("1"));
            Assert.That(redisValues[1].Value, Is.EqualTo("2"));
            Assert.That(redisValues[2].Value, Is.EqualTo("3"));
            
            var ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("MultiKeyWithTtl1");
            Assert.That(ttl, Is.LessThanOrEqualTo(120).And.Positive);
            
            ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("MultiKeyWithTtl2");
            Assert.That(ttl, Is.LessThanOrEqualTo(120).And.Positive);
            
            ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("MultiKeyWithTtl3");
            Assert.That(ttl, Is.LessThanOrEqualTo(120).And.Positive);
        }

        [Test]
        [Repeat(2)]
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
            var response = await _tomLonghurstRedisClient.Cluster.ClusterInfoAsync();
            var firstLine = response.Split("\n").First();
            Assert.That(firstLine, Is.EqualTo("cluster_state:ok"));
        }

        // Needs Access to Socket (which is private) to Close it
        [Test]
        [Repeat(2)]
        public async Task Disconnected()
        {
            var client = await _redisManager.GetRedisClientAsync();
            await client.StringSetAsync("DisconnectTest", "123", 120, AwaitOptions.AwaitCompletion);
            var redisValue = await client.StringGetAsync("DisconnectTest");
            Assert.AreEqual("123", redisValue.Value);
            
            client.Socket.Close();
            
            var result = await client.StringGetAsync("DisconnectTest");
            
            Assert.AreEqual("123", result.Value);
        }

        [TestCase("IncrKey")]
        [Repeat(2)]
        public async Task Incr(string key)
        {
            await _tomLonghurstRedisClient.DeleteKeyAsync(key, AwaitOptions.AwaitCompletion);
            
            var one = await _tomLonghurstRedisClient.IncrementAsync(key);
            Assert.That(one, Is.EqualTo(1));
            
            var three = await _tomLonghurstRedisClient.IncrementByAsync(key, 2);
            Assert.That(three, Is.EqualTo(3));
            
            var threeAndAHalf = await _tomLonghurstRedisClient.IncrementByAsync(key, 0.5f);
            Assert.That(threeAndAHalf, Is.EqualTo(3.5f));
            
            var four = await _tomLonghurstRedisClient.IncrementByAsync(key, 0.5f);
            Assert.That(four, Is.EqualTo(4f));
            
            var five = await _tomLonghurstRedisClient.IncrementAsync(key);
            Assert.That(five, Is.EqualTo(5));
            
            await _tomLonghurstRedisClient.ExpireAsync(key, 120);
        }

        [Test]
        [Repeat(2)]
        public async Task Info()
        {
            var info = await _tomLonghurstRedisClient.Server.Info();
        }
        
        [Test]
        [Repeat(2)]
        public async Task DBSize()
        {
            var dbSize = await _tomLonghurstRedisClient.Server.DBSize();
        }

        [TestCase("DecrKey")]
        [Repeat(2)]
        public async Task Decr(string key)
        {
            await Incr(key);
         
            var four = await _tomLonghurstRedisClient.DecrementAsync(key);
            Assert.That(four, Is.EqualTo(4));
            
            var two = await _tomLonghurstRedisClient.DecrementByAsync(key, 2);
            Assert.That(two, Is.EqualTo(2));
            
            var one = await _tomLonghurstRedisClient.DecrementAsync(key);
            Assert.That(one, Is.EqualTo(1));
        }

        [Test]
        [Repeat(2)]
        public async Task Expire()
        {
            await _tomLonghurstRedisClient.StringSetAsync("ExpireKey", "123", AwaitOptions.AwaitCompletion);
            var ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.EqualTo(-1));
            
            await _tomLonghurstRedisClient.ExpireAsync("ExpireKey", 30);
            ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.LessThanOrEqualTo(30));
            
            await _tomLonghurstRedisClient.PersistAsync("ExpireKey");
            ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.EqualTo(-1));
            
            await _tomLonghurstRedisClient.ExpireAsync("ExpireKey", 30);
            ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.LessThanOrEqualTo(30));
        }
        
        [Test]
        [Repeat(2)]
        public async Task ExpireAt()
        {
            await _tomLonghurstRedisClient.StringSetAsync("ExpireKeyDateTime", "123", AwaitOptions.AwaitCompletion);
            await _tomLonghurstRedisClient.ExpireAtAsync("ExpireKeyDateTime", DateTimeOffset.Now.AddSeconds(30));
            var ttl = await _tomLonghurstRedisClient.TimeToLiveAsync("ExpireKeyDateTime");
            Assert.That(ttl, Is.LessThanOrEqualTo(30));
        }
    }
}