using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Models.RequestModels;

namespace RedisClientTest
{
    public class Tests
    {
        private RedisClientManager _redisManager;
        private RedisClientConfig _config;
        private Task<RedisClient> TomLonghurstRedisClient => _redisManager.GetRedisClientAsync();

        // To test, create a Test Information static class to store your Host, Port and Password for your Test Redis Server
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new RedisClientConfig(TestInformation.Host, TestInformation.Port,
                TestInformation.Password)
            {
                Ssl = false
            };
            _redisManager = new RedisClientManager(_config, 1);
            _redisManager.GetAllRedisClients();
        }

        [SetUp]
        public void BeforeTest()
        {
            Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
        }

        [TestCase("value with a space")]
        [TestCase("value with two  spaces")]
        [TestCase("value")]
        [TestCase("value with a\nnew line")]
        [TestCase("value with a\r\nnew line")]
        [Repeat(2)]
        public async Task Values(string value)
        {
            await (await TomLonghurstRedisClient).StringSetAsync("key", value);
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
            
            await (await TomLonghurstRedisClient).StringSetAsync(data);
            
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
        [TestCase("LargeValue", "large_json.json")]
        [TestCase("LargeValue2", "large_json2.json")]
        public async Task LargeValue(string key, string filename)
        {
            var largeValueJson = await File.ReadAllTextAsync(filename);
            await (await TomLonghurstRedisClient).StringSetAsync(key, largeValueJson);
            var result = await (await TomLonghurstRedisClient).StringGetAsync(key);
            await (await TomLonghurstRedisClient).ExpireAsync(key, 120);
            
            Assert.AreEqual(largeValueJson, result.Value);
        }

        [Ignore("")]
        [Test]
        public async Task MultipleThreads()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Factory.StartNew(async () => await LargeValue($"MultiThread{i}", "large_json.json")).Unwrap());
            }

            await Task.WhenAll(tasks);
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
                            var client = await _redisManager.GetRedisClientAsync();
                            
                            await Time("Set", async delegate
                            {
                                await client.StringSetAsync($"MemoryTestKey{i1}", largeJsonContents, 120);
                            });

                            await Time("Get", async delegate
                            {
                                var result = await client.StringGetAsync($"MemoryTestKey{i1}");
                                Assert.That(result.Value, Is.EqualTo(largeJsonContents));
                            });

                            await Time("Delete", async delegate
                            {
                                await client.DeleteKeyAsync($"MultiTestKey{i1}");
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
        
        [Ignore("")]
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
                            120);

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

        [TestCase(TestClient.StackExchange)]
        [TestCase(TestClient.TomLonghurst)]
        public async Task PerformanceTest2(TestClient testClient)
        {
            var tasks = new List<Task>();
            if (testClient == TestClient.StackExchange)
            {
                var stackExchange = (await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
                {
                    EndPoints = {{TestInformation.Host, TestInformation.Port}},
                    Password = TestInformation.Password,
                    Ssl = false
                })).GetDatabase(0);

                await stackExchange.StringSetAsync("SingleKey", "123", TimeSpan.FromSeconds(120));

                var stackExchangeRedisClientStopwatch = Stopwatch.StartNew();

                for (var i = 0; i < 200; i++)
                {
                    tasks.Add(stackExchange.StringGetAsync("SingleKey"));
                }

                await Task.WhenAll(tasks);

                stackExchangeRedisClientStopwatch.Stop();
                var stackExchangeRedisClientStopwatchTimeTaken =
                    stackExchangeRedisClientStopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Time Taken: {stackExchangeRedisClientStopwatchTimeTaken} ms");
            }
            else
            {
                var client = await TomLonghurstRedisClient;
                await client.StringSetAsync("SingleKey",
                    "123",
                    120);

                var tomLonghurstRedisClientStopwatch = Stopwatch.StartNew();

                for (var i = 0; i < 200; i++)
                {
                    tasks.Add(client.StringGetAsync("SingleKey"));
                }
                
                await Task.WhenAll(tasks);

                tomLonghurstRedisClientStopwatch.Stop();
                var tomLonghurstRedisClientStopwatchTimeTaken =
                    tomLonghurstRedisClientStopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Time Taken: {tomLonghurstRedisClientStopwatchTimeTaken} ms");
            }
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
            
            (await TomLonghurstRedisClient).StringSetAsync("TestyMcTestFace", "123", 120);
            (await TomLonghurstRedisClient).StringSetAsync("TestyMcTestFace2", "1234", 120);
            (await TomLonghurstRedisClient).StringSetAsync("TestyMcTestFace3", "12345", 120);
            
            var getValue = await (await TomLonghurstRedisClient).StringGetAsync(new [] { "TestyMcTestFace", "TestyMcTestFace2", "TestyMcTestFace3" });
            Assert.That(getValue.Count(), Is.EqualTo(3));
            
            var getValueSingle = await (await TomLonghurstRedisClient).StringGetAsync("TestyMcTestFace");
            Assert.That(getValueSingle.Value, Is.EqualTo("123"));

            
            await (await TomLonghurstRedisClient).StringSetAsync("KeyExists", "123");
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
            await (await TomLonghurstRedisClient).StringSetAsync("Exists", "123", 30);
            var values = (await (await TomLonghurstRedisClient).StringGetAsync(new [] { "Blah1", "Blah2", "Exists", "Blah4", "Blah5" })).ToList();
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values.Count(value => value.HasValue), Is.EqualTo(1));
        }
        
        [Test]
        [Repeat(2)]
        public async Task SetGetDeleteSingleKey()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("SingleKey", "123");
            var redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKey");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
            
            await (await TomLonghurstRedisClient).DeleteKeyAsync("SingleKey");
            redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKey");
            Assert.That(redisValue.HasValue, Is.EqualTo(false));
        }

        [Test]
        [Repeat(2)]
        public async Task SetGetSingleKeyWithTtl()
        {
            await (await TomLonghurstRedisClient).StringSetAsync("SingleKeyWithTtl", "123", 30);
            var redisValue = await (await TomLonghurstRedisClient).StringGetAsync("SingleKeyWithTtl");
            Assert.That(redisValue.Value, Is.EqualTo("123"));
        }

        [Test]
        [Repeat(2)]
        public async Task SetMultipleTtl()
        {
            var client = await TomLonghurstRedisClient;
            await client.StringSetAsync(new List<RedisKeyValue>
                {
                    new RedisKeyValue("BlahTTL1", "Blah1"),
                    new RedisKeyValue("BlahTTL2", "Blah2"),
                    new RedisKeyValue("BlahTTL3", "Blah3"),
                    new RedisKeyValue("BlahTTL4", "Blah4"),
                    new RedisKeyValue("BlahTTL5", "Blah5")
                },
                120);

            var ttl = await client.TimeToLiveAsync("BlahTTL1");
            Assert.That(ttl, Is.Positive.And.LessThanOrEqualTo(125));
        }
        
        [Test]
        [Repeat(2)]
        public async Task MultipleExpire()
        {
            var client = await TomLonghurstRedisClient;
            await client.StringSetAsync(new List<RedisKeyValue>
                {
                    new RedisKeyValue("BlahExpire1", "Blah1"),
                    new RedisKeyValue("BlahExpire2", "Blah2"),
                    new RedisKeyValue("BlahExpire3", "Blah3"),
                    new RedisKeyValue("BlahExpire4", "Blah4"),
                    new RedisKeyValue("BlahExpire5", "Blah5")
                });

            await client.ExpireAsync(new List<string>
            {
                "BlahExpire1",
                "BlahExpire2",
                "BlahExpire3",
                "BlahExpire4",
                "BlahExpire5"
            }, 120);

            var ttl = await client.TimeToLiveAsync("BlahExpire1");
            Assert.That(ttl, Is.Positive.And.LessThanOrEqualTo(125));
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
            
            await (await TomLonghurstRedisClient).StringSetAsync(keyValues);
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
            var results = keys.Select(key => tomLonghurstRedisClient.StringSetAsync(key, "123", 30));
            
            await Task.WhenAll(results);

            foreach (var key in keys)
            {
                var value = await tomLonghurstRedisClient.StringGetAsync(key);
                Assert.That(value.Value, Is.EqualTo("123"));
            }
        }

        [Test]
        [Repeat(2)]
        public async Task SetGetMultipleKeyWithTtlMultiple()
        {
            await SetGetMultipleKeyWithTtl();
            await SetGetMultipleKeyWithTtl();
            await SetGetMultipleKeyWithTtl();
            await SetGetMultipleKeyWithTtl();
            await SetGetMultipleKeyWithTtl();
        }
        
        [Test]
        [Repeat(2)]
        public async Task SetGetMultipleKeyWithTtlMultipleWithFireAndForget()
        {
            SetGetMultipleKeyWithTtl();
            SetGetMultipleKeyWithTtl();
            SetGetMultipleKeyWithTtl();
            SetGetMultipleKeyWithTtl();
            await SetGetMultipleKeyWithTtl();
        }
        
        [Test]
        [Repeat(2)]
        public async Task SetGetMultipleKeyWithTtl()
        {
            var keyValues = new List<RedisKeyValue>
            {
                new RedisKeyValue("MultiKeyWithTtl1", "1"),
                new RedisKeyValue("MultiKeyWithTtl2", "2"),
                new RedisKeyValue("MultiKeyWithTtl3", "3")
            };
            
            await (await TomLonghurstRedisClient).StringSetAsync(keyValues, 120);
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
            await (await TomLonghurstRedisClient).StringSetAsync("KeyExistsCheck", "123", 30);
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
            var client = await _redisManager.GetRedisClientAsync();
            await client.StringSetAsync("DisconnectTest", "123", 120);
            var redisValue = await client.StringGetAsync("DisconnectTest");
            Assert.AreEqual("123", redisValue.Value);
            
            client.Socket.Close();

            await Task.Delay(1000);
            
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

            var client1 = await _redisManager.GetRedisClientAsync();
            var client2 = await _redisManager.GetRedisClientAsync();
            var client3 = await _redisManager.GetRedisClientAsync();
            
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
            await (await TomLonghurstRedisClient).DeleteKeyAsync(key);
            
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
            await (await TomLonghurstRedisClient).StringSetAsync("ExpireKey", "123");
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
            await (await TomLonghurstRedisClient).StringSetAsync("ExpireKeyDateTime", "123");
            await (await TomLonghurstRedisClient).ExpireAtAsync("ExpireKeyDateTime", DateTimeOffset.Now.AddSeconds(30));
            var ttl = await (await TomLonghurstRedisClient).TimeToLiveAsync("ExpireKeyDateTime");
            Assert.That(ttl, Is.LessThanOrEqualTo(33));
        }

        [Test]
        [Repeat(2)]
        public async Task Mix()
        {
            var tasks = new List<Task>();
            
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await SetGetMultipleKey();
                    await Pipelining_Multiple_Sets();
                    await GetKeys();
                    await GetNonExistingKey();
                    await GetKeys();
                    await SetGetMultipleKey();
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}