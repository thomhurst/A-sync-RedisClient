using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
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
        private Task<RedisClient> TomLonghurstRedisClient => _redisManager.GetRedisClient();

        // To test, create a Test Information static class to store your Host, Port and Password for your Test Redis Server
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new RedisClientConfig(TestInformation.Host, TestInformation.Port,
                TestInformation.Password)
            {
                Ssl = false
            };
            _redisManager = new RedisClientManager(_config, 5);
            _redisManager.GetAllRedisClients();
        }

        [TestCase("value with a space")]
        [TestCase("value with two  spaces")]
        [TestCase("value")]
        [TestCase("value with a\nnew line")]
        [TestCase("value with a\r\nnew line")]
        [Repeat(2)]
        public async Task Values(string value)
        {
            await (await TomLonghurstRedisClient).StringSetAsync("key", value, AwaitOptions.AwaitCompletion);
            var redisValue = await (await TomLonghurstRedisClient).StringGetAsync("key");
            
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
            
            await (await TomLonghurstRedisClient).StringSetAsync(data, AwaitOptions.AwaitCompletion);
            
            var redisValue1 = await (await TomLonghurstRedisClient).StringGetAsync("key1");
            Assert.That(redisValue1.Value, Is.EqualTo("value with a space1"));
            
            var redisValue2 = await (await TomLonghurstRedisClient).StringGetAsync("key2");
            Assert.That(redisValue2.Value, Is.EqualTo("value with a space2"));
            
            var redisValue3 = await (await TomLonghurstRedisClient).StringGetAsync("key3");
            Assert.That(redisValue3.Value, Is.EqualTo("value with a space3"));
        }

        public async Task Time(string title, Func<Task> action)
        {
            var sw = Stopwatch.StartNew();
            await action.Invoke();
            sw.Stop();
            Console.WriteLine($"{title} - Time Taken: {sw.ElapsedMilliseconds} ms");
        }

        [Test]
        [Repeat(2)]
        public async Task LargeValue()
        {
            var client = await _redisManager.GetRedisClient();
            var largeValueJson = await File.ReadAllTextAsync("large_json.json");
            await client.StringSetAsync("LargeValue", largeValueJson, AwaitOptions.AwaitCompletion);
            var result = await client.StringGetAsync($"LargeValue");
            await client.ExpireAsync("LargeValue", 120);
            
            Assert.AreEqual(largeValueJson, result.Value);
        }
        
        //[Test]
        public async Task MemoryTest()
        {
            string ver = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

            var largeJsonContents = await File.ReadAllTextAsync("large_json.json");
            var sw = Stopwatch.StartNew();
            var tasks = new List<Task>();

            for (int i = 0; i < 50; i++)
            {
                var i1 = i;
                var task = Task.Run(async () =>
                {
                    while (sw.Elapsed < TimeSpan.FromMinutes(1))
                    {
                        try
                        {
                            var client = await _redisManager.GetRedisClient();
                            
                            await Time("Set", async delegate
                            {
                                await client.StringSetAsync($"MemoryTestKey{i1}", largeJsonContents, 120,
                                    AwaitOptions.AwaitCompletion);
                            });

                            await Time("Get", async delegate
                            {
                                var result = await client.StringGetAsync($"MemoryTestKey{i1}");
                                Assert.That(result.Value, Is.EqualTo(largeJsonContents));
                            });

                            await Time("Delete", async delegate
                            {
                                await client.DeleteKeyAsync($"MultiTestKey{i1}", AwaitOptions.AwaitCompletion);
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("Finished.");
        }

        public enum TestClient
        {
            StackExchange,
            TomLonghurst
        }
        
        //[Ignore("")]
       [TestCase(TestClient.StackExchange)]
        [TestCase(TestClient.TomLonghurst)]
        public async Task PerformanceTest(TestClient testClient)
        {
            var tasks = new List<Task>();

            for (var taskCount = 0; taskCount < 5; taskCount++)
            {
                var task = Task.Run(async () =>
                {
                    if (testClient == TestClient.StackExchange)
                    {
                        var stackExchange = (await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
                        {
                            EndPoints = {{TestInformation.Host, TestInformation.Port}},
                            Password = TestInformation.Password,
                            Ssl = false
                        })).GetDatabase(0);

                        await stackExchange.StringSetAsync("SingleKey", "123", TimeSpan.FromSeconds(120));

                        for (var outer = 0; outer < 5; outer++)
                        {
                            var stackExchangeRedisClientStopwatch = Stopwatch.StartNew();

                            for (var i = 0; i < 200; i++)
                            {
                                var redisValue = await stackExchange.StringGetAsync("SingleKey");
                            }

                            stackExchangeRedisClientStopwatch.Stop();
                            var stackExchangeRedisClientStopwatchTimeTaken =
                                stackExchangeRedisClientStopwatch.ElapsedMilliseconds;
                            Console.WriteLine($"Time Taken: {stackExchangeRedisClientStopwatchTimeTaken} ms");
                        }
                    }
                    else
                    {
                        await (await TomLonghurstRedisClient).StringSetAsync("SingleKey",
                            "123",
                            120,
                            AwaitOptions.AwaitCompletion);

                        for (var outer = 0; outer < 5; outer++)
                        {
                            var tomLonghurstRedisClientStopwatch = Stopwatch.StartNew();

                            for (var i = 0; i < 200; i++)
                            {
                                var redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKey");
                            }

                            tomLonghurstRedisClientStopwatch.Stop();
                            var tomLonghurstRedisClientStopwatchTimeTaken =
                                tomLonghurstRedisClientStopwatch.ElapsedMilliseconds;
                            Console.WriteLine($"Time Taken: {tomLonghurstRedisClientStopwatchTimeTaken} ms");
                        }
                    }
                });
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        [Test]
        [Repeat(2)]
        public async Task Test1Async()
        {
            var sw = Stopwatch.StartNew();
            
            var pong = await (await TomLonghurstRedisClient).Ping();
            Assert.AreEqual(true, pong.IsSuccessful);
            
            var getDoesntExist = (await (await TomLonghurstRedisClient).StringGetAsync(new [] { "Blah1", "Blah2" })).ToList();
            Assert.That(getDoesntExist.Count, Is.EqualTo(2));
            Assert.That(getDoesntExist.Count(value => value.HasValue), Is.EqualTo(0));
            
            await (await TomLonghurstRedisClient).StringSetAsync("TestyMcTestFace", "123", 120, AwaitOptions.FireAndForget);
            await (await TomLonghurstRedisClient).StringSetAsync("TestyMcTestFace2", "1234", 120, AwaitOptions.FireAndForget);
            await (await TomLonghurstRedisClient).StringSetAsync("TestyMcTestFace3", "12345", 120, AwaitOptions.FireAndForget);
            
            var getValue = await (await TomLonghurstRedisClient).StringGetAsync(new [] { "TestyMcTestFace", "TestyMcTestFace2", "TestyMcTestFace3" });
            Assert.That(getValue.Count(), Is.EqualTo(3));
            
            var getValueSingle = await (await TomLonghurstRedisClient).StringGetAsync("TestyMcTestFace");
            Assert.That(getValueSingle.Value, Is.EqualTo("123"));

            
            await (await TomLonghurstRedisClient).StringSetAsync("KeyExists", "123", AwaitOptions.AwaitCompletion);
            var keyExistsFalse = await (await TomLonghurstRedisClient).KeyExistsAsync("KeyDoesntExist");
            Assert.That(keyExistsFalse, Is.EqualTo(false));
            var keyExistsTrue = await (await TomLonghurstRedisClient).KeyExistsAsync("KeyExists");
            Assert.That(keyExistsTrue, Is.EqualTo(true));
            
            var timeTaken = sw.ElapsedMilliseconds;
            Console.WriteLine($"Time Taken: {timeTaken} ms");
        }
        
        [Test]
        [Repeat(2)]
        public async Task Ping()
        {
            var pong = await (await TomLonghurstRedisClient).Ping(); 
            
            Assert.AreEqual(true, pong.IsSuccessful);
            Assert.AreEqual("PONG", pong.Message);
            
            Console.WriteLine($"Time Taken: {pong.TimeTaken.TotalMilliseconds} ms");
        }
        
        [Test]
        [Repeat(2)]
        public async Task GetNonExistingKey()
        {
            var nonExistingKey = await (await TomLonghurstRedisClient).StringGetAsync("Blah1");
            Assert.That(nonExistingKey.HasValue, Is.False);
        }
        
        [Test]
        [Repeat(2)]
        public async Task GetNonExistingKeys()
        {
            var nonExistingKeys = (await (await TomLonghurstRedisClient).StringGetAsync(new [] { "Blah1", "Blah2" })).ToList();
            Assert.That(nonExistingKeys.Count, Is.EqualTo(2));
            Assert.That(nonExistingKeys.Count(value => value.HasValue), Is.EqualTo(0));
        }
        
        [Test]
        [Repeat(2)]
        public async Task GetExistingKeyAmongstNonExistingKeys()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("Exists", "123", 30, AwaitOptions.AwaitCompletion);
            var values = (await (await TomLonghurstRedisClient).StringGetAsync(new [] { "Blah1", "Blah2", "Exists", "Blah4", "Blah5" })).ToList();
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values.Count(value => value.HasValue), Is.EqualTo(1));
        }
        
        [Test]
        [Repeat(2)]
        public async Task SetGetDeleteSingleKey()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("SingleKey", "123", AwaitOptions.AwaitCompletion);
            var redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKey");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
            
            await (await TomLonghurstRedisClient).DeleteKeyAsync("SingleKey", AwaitOptions.AwaitCompletion);
            redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKey");
            Assert.That(redisValue.HasValue, Is.EqualTo(false));
        }

        [Test]
        [Repeat(2)]
        public async Task SetGetSingleKeyWithTtl()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("SingleKeyWithTtl", "123", 30, AwaitOptions.AwaitCompletion);
            var redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKeyWithTtl");
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
            
            await (await TomLonghurstRedisClient).StringSetAsync(keyValues, AwaitOptions.AwaitCompletion);
            var redisValues = (await (await TomLonghurstRedisClient).StringGetAsync(new [] { "MultiKey1", "MultiKey2", "MultiKey3" })).ToList();
            Assert.That(redisValues.Count(), Is.EqualTo(3));
            Assert.That(redisValues[0].Value, Is.EqualTo("1"));
            Assert.That(redisValues[1].Value, Is.EqualTo("2"));
            Assert.That(redisValues[2].Value, Is.EqualTo("3"));
        }

        [Test]
        [Repeat(2)]
        public async Task Pipelining_Multiple_Sets()
        {
            var tomLonghurstRedisClient = await TomLonghurstRedisClient;
            
            var keys = new[]
            {
                "Pipeline1", "Pipeline2", "Pipeline3", "Pipeline4", "Pipeline5", "Pipeline6", "Pipeline7", "Pipeline8"
            };
            var results = keys.Select(key => tomLonghurstRedisClient.StringSetAsync(key, "123", 30, AwaitOptions.FireAndForget));
            
            await Task.WhenAll(results);

            foreach (var key in keys)
            {
                var value = await tomLonghurstRedisClient.StringGetAsync(key);
                Assert.That(value.Value, Is.EqualTo("123"));
            }
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
            
            await (await TomLonghurstRedisClient).StringSetAsync(keyValues, 120, awaitOptions);
            var redisValues = (await (await TomLonghurstRedisClient).StringGetAsync(new [] { "MultiKeyWithTtl1", "MultiKeyWithTtl2", "MultiKeyWithTtl3" })).ToList();
            Assert.That(redisValues.Count, Is.EqualTo(3));
            Assert.That(redisValues[0].Value, Is.EqualTo("1"));
            Assert.That(redisValues[1].Value, Is.EqualTo("2"));
            Assert.That(redisValues[2].Value, Is.EqualTo("3"));
            
            var ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("MultiKeyWithTtl1");
            Assert.That(ttl, Is.LessThanOrEqualTo(120).And.Positive);
            
            ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("MultiKeyWithTtl2");
            Assert.That(ttl, Is.LessThanOrEqualTo(120).And.Positive);
            
            ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("MultiKeyWithTtl3");
            Assert.That(ttl, Is.LessThanOrEqualTo(120).And.Positive);
        }

        [Test]
        [Repeat(2)]
        public async Task KeyExists()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("KeyExistsCheck", "123", 30, AwaitOptions.AwaitCompletion);
            var exists = await (await TomLonghurstRedisClient).KeyExistsAsync("KeyExistsCheck");
            var doesntExist = await (await TomLonghurstRedisClient).KeyExistsAsync("KeyDoesntExistsCheck");
            
            Assert.That(exists, Is.EqualTo(true));
            Assert.That(doesntExist, Is.EqualTo(false));
        }
        
        [Test, Ignore("No Cluster Support on Redis Server")]
        public async Task ClusterInfo()
        {
            var response = await (await TomLonghurstRedisClient).Cluster.ClusterInfoAsync();
            var firstLine = response.Split("\n").First();
            Assert.That(firstLine, Is.EqualTo("cluster_state:ok"));
        }

        // Needs Access to Socket (which is private) to Close it
        [Test]
        [Repeat(2)]
        public async Task Disconnected()
        {
            var client = await _redisManager.GetRedisClient();
            await client.StringSetAsync("DisconnectTest", "123", 120, AwaitOptions.AwaitCompletion);
            var redisValue = await client.StringGetAsync("DisconnectTest");
            Assert.AreEqual("123", redisValue.Value);
            
            client.Socket.Close();
            
            var result = await client.StringGetAsync("DisconnectTest");
            
            Assert.AreEqual("123", result.Value);
        }

        [Test]
        public async Task GetKey()
        {
            var result =
                await (await TomLonghurstRedisClient).StringGetAsync(
                    "SummaryProduct_V3_1022864315_1724593328_COM_GBP_UK_en-GB_");
            
            Console.Write(result);
        }
        
        [Test]
        public async Task GetKeys()
        {
            var keys = GenerateMassKeys();

            var client1 = await _redisManager.GetRedisClient();
            var client2 = await _redisManager.GetRedisClient();
            var client3 = await _redisManager.GetRedisClient();
            
            var resultTask =
                client1.StringGetAsync(keys);
            
            var result2Task =
                client2.StringGetAsync(keys);
            
            var result3Task =
                client3.StringGetAsync(keys);

            var result = await Task.WhenAll(resultTask, result2Task, result3Task);

            var result1 = result[0].ToList();
            var result2 = result[1].ToList();
            var result3 = result[2].ToList();
            
            Console.Write(result1);
        }

        private List<string> GenerateMassKeys()
        {
            var list = new List<string>();
            for (var i = 0; i < 50; i++)
            {
                list.AddRange(
                    new[]
                    {
                        "SummaryProduct_V3_1099538108_1873678337_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1216417282_216646695_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1471008232__COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1431558723__COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1526680692_724222009_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1650151356_234415250_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1650151356_778891134_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1798679041_1695172977_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1809834294_1582795796_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_183736170_969769947_COM_GBP_UK_en-GB_ckp5egq-11",
                        "SummaryProduct_V3_1733691802_1464284012_COM_GBP_UK_en-GB_ckp5egq-11"
                    }
                );
            }

            return list;
        }

        [TestCase("IncrKey")]
        [Repeat(2)]
        public async Task Incr(string key)
        {
            await (await TomLonghurstRedisClient).DeleteKeyAsync(key, AwaitOptions.AwaitCompletion);
            
            var one = await (await TomLonghurstRedisClient).IncrementAsync(key);
            Assert.That(one, Is.EqualTo(1));
            
            var three = await (await TomLonghurstRedisClient).IncrementByAsync(key, 2);
            Assert.That(three, Is.EqualTo(3));
            
            var threeAndAHalf = await (await TomLonghurstRedisClient).IncrementByAsync(key, 0.5f);
            Assert.That(threeAndAHalf, Is.EqualTo(3.5f));
            
            var four = await (await TomLonghurstRedisClient).IncrementByAsync(key, 0.5f);
            Assert.That(four, Is.EqualTo(4f));
            
            var five = await (await TomLonghurstRedisClient).IncrementAsync(key);
            Assert.That(five, Is.EqualTo(5));
            
            await (await TomLonghurstRedisClient).ExpireAsync(key, 120);
        }

        [TestCase("IncrKey")]
        [Repeat(2)]
        public async Task IncrLots(string key)
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Incr($"{key}{i}"));
            }

            await Task.WhenAll(tasks);
        }

        [Test]
        [Repeat(2)]
        public async Task Info()
        {
            var info = await (await TomLonghurstRedisClient).Server.Info();
        }
        
        [Test]
        [Repeat(2)]
        public async Task DBSize()
        {
            var tomLonghurstRedisClient = await TomLonghurstRedisClient;
            var dbSize = await tomLonghurstRedisClient.Server.DBSize();
        }

        [TestCase("DecrKey")]
        [Repeat(2)]
        public async Task Decr(string key)
        {
            await Incr(key);

            var four = await (await TomLonghurstRedisClient).DecrementAsync(key);
            Assert.That(four, Is.EqualTo(4));
            
            var two = await (await TomLonghurstRedisClient).DecrementByAsync(key, 2);
            Assert.That(two, Is.EqualTo(2));
            
            var one = await (await TomLonghurstRedisClient).DecrementAsync(key);
            Assert.That(one, Is.EqualTo(1));
        }

        [Test]
        [Repeat(2)]
        public async Task Expire()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("ExpireKey", "123", AwaitOptions.AwaitCompletion);
            var ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.EqualTo(-1));
            
            await (await TomLonghurstRedisClient).ExpireAsync("ExpireKey", 30);
            ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.LessThanOrEqualTo(30));
            
            await (await TomLonghurstRedisClient).PersistAsync("ExpireKey");
            ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.EqualTo(-1));
            
            await (await TomLonghurstRedisClient).ExpireAsync("ExpireKey", 30);
            ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("ExpireKey");
            Assert.That(ttl, Is.LessThanOrEqualTo(30));
        }
        
        [Test]
        [Repeat(2)]
        public async Task ExpireAt()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("ExpireKeyDateTime", "123", AwaitOptions.AwaitCompletion);
            await (await TomLonghurstRedisClient).ExpireAtAsync("ExpireKeyDateTime", DateTimeOffset.Now.AddSeconds(30));
            var ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("ExpireKeyDateTime");
            Assert.That(ttl, Is.LessThanOrEqualTo(33));
        }
    }
}