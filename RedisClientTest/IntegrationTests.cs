using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using StackExchange.Redis;
using TomLonghurst.AsyncRedisClient.Client;
using TomLonghurst.AsyncRedisClient.Models.RequestModels;
using Assembly = System.Reflection.Assembly;

namespace RedisClientTest;

public class Tests : TestBase
{
    private RedisClientManager _redisManager;
    private RedisClientConfig _config;

    public RedisClient Client => _redisManager.GetRedisClient();

    [Before(Test)]
    public async Task Setup()
    {
        _config = new RedisClientConfig(Host, Port)
        {
            Ssl = false
        };
        
        _redisManager = await RedisClientManager.ConnectAsync(_config);
    }

    [Arguments("value with a space")]
    [Arguments("value with two  spaces")]
    [Arguments("value")]
    [Arguments("value with a\nnew line")]
    [Arguments("value with a\r\nnew line")]
    [Repeat(2)]
    [Test]
    public async Task Values(string value)
    {
        await Client.StringSetAsync("key", value);
        var redisValue = await Client.StringGetAsync("key");
            
        await Assert.That(redisValue.Value).IsEqualTo(value);
    }
        
    [Test]
    [Repeat(2)]
    public async Task Multiple_Values_With_Space()
    {
        var data = new List<RedisKeyValue>
        {
            new("key1", "value with a space1"),
            new("key2", "value with a space2"),
            new("key3", "value with a space3")
        };
            
        await Client.StringSetAsync(data);
            
        var redisValue1 = await Client.StringGetAsync("key1");
        await Assert.That(redisValue1.Value).IsEqualTo("value with a space1");
            
        var redisValue2 = await Client.StringGetAsync("key2");
        await Assert.That(redisValue2.Value).IsEqualTo("value with a space2");
            
        var redisValue3 = await Client.StringGetAsync("key3");
        await Assert.That(redisValue3.Value).IsEqualTo("value with a space3");
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
    [Arguments("LargeValue", "large_json.json")]
    [Arguments("LargeValue2", "large_json2.json")]
    public async Task LargeValue(string key, string filename)
    {
        var largeValueJson = await File.ReadAllTextAsync(filename);
        await Client.StringSetAsync(key, largeValueJson);
        var result = await Client.StringGetAsync(key);
        await Client.ExpireAsync(key, 120);
            
        await Assert.That(largeValueJson).IsEqualTo(result.Value);
    }

    [Skip("")]
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
                        var client = _redisManager.GetRedisClient();
                            
                        await Time("Set", async delegate
                        {
                            await client.StringSetAsync($"MemoryTestKey{i1}", largeJsonContents, 120);
                        });

                        await Time("Get", async delegate
                        {
                            var result = await client.StringGetAsync($"MemoryTestKey{i1}");
                            await Assert.That(result.Value).IsEqualTo(largeJsonContents);
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
        
    [Skip("")]
    [Arguments(TestClient.StackExchange)]
    [Arguments(TestClient.TomLonghurst)]
    [Test]
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
                        EndPoints = {{Host, Port}},
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
                    await Client.StringSetAsync("SingleKey",
                        "123",
                        120);

                    for (var outer = 0; outer < 5; outer++)
                    {
                        var tomLonghurstRedisClientStopwatch = Stopwatch.StartNew();

                        for (var i = 0; i < 200; i++)
                        {
                            var redisValue = await Client.StringGetAsync("SingleKey");
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

    [Arguments(TestClient.StackExchange)]
    [Arguments(TestClient.TomLonghurst)]
    [Test]
    public async Task PerformanceTest2(TestClient testClient)
    {
        var tasks = new List<Task>();
        if (testClient == TestClient.StackExchange)
        {
            var stackExchange = (await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
            {
                EndPoints = {{Host, Port}},
                Ssl = true
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
            var client = Client;
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
            
        var pong = await Client.Ping();
        await Assert.That(pong.IsSuccessful).IsTrue();
            
        var getDoesntExist = (await Client.StringGetAsync(["Blah1", "Blah2"])).ToList();
        await Assert.That(getDoesntExist.Count).IsEqualTo(2);
        await Assert.That(getDoesntExist.Count(value => value.HasValue)).IsEqualTo(0);
            
        await Client.StringSetAsync("TestyMcTestFace", "123", 120);
        await Client.StringSetAsync("TestyMcTestFace2", "1234", 120);
        await Client.StringSetAsync("TestyMcTestFace3", "12345", 120);
            
        var getValue = await Client.StringGetAsync(["TestyMcTestFace", "TestyMcTestFace2", "TestyMcTestFace3"
        ]);
        await Assert.That(getValue.Count()).IsEqualTo(3);
            
        var getValueSingle = await Client.StringGetAsync("TestyMcTestFace");
        await Assert.That(getValueSingle.Value).IsEqualTo("123");

            
        await Client.StringSetAsync("KeyExists", "123");
        var keyExistsFalse = await Client.KeyExistsAsync("KeyDoesntExist");
        await Assert.That(keyExistsFalse).IsEqualTo(false);
        var keyExistsTrue = await Client.KeyExistsAsync("KeyExists");
        await Assert.That(keyExistsTrue).IsEqualTo(true);
            
        var timeTaken = sw.ElapsedMilliseconds;
        Console.WriteLine($"Time Taken: {timeTaken} ms");
    }
        
    [Test]
    [Repeat(2)]
    public async Task Ping()
    {
        var pong = await Client.Ping(); 
            
        await Assert.That(pong.IsSuccessful).IsTrue();
        await Assert.That(pong.Message).IsEqualTo("PONG");
            
        Console.WriteLine($"Time Taken: {pong.TimeTaken.TotalMilliseconds} ms");
    }
        
    [Test]
    [Repeat(2)]
    public async Task GetNonExistingKey()
    {
        var nonExistingKey = await Client.StringGetAsync("Blah1");
        await Assert.That(nonExistingKey.HasValue).IsFalse();
    }
        
    [Test]
    [Repeat(2)]
    public async Task GetNonExistingKeys()
    {
        var nonExistingKeys = (await Client.StringGetAsync(["Blah1", "Blah2"])).ToList();
        await Assert.That(nonExistingKeys.Count).IsEqualTo(2);
        await Assert.That(nonExistingKeys.Count(value => value.HasValue)).IsEqualTo(0);
    }
        
    [Test]
    [Repeat(2)]
    public async Task GetExistingKeyAmongstNonExistingKeys()
    {
        await Client.StringSetAsync("Exists", "123", 30);
        var values = (await Client.StringGetAsync(["Blah1", "Blah2", "Exists", "Blah4", "Blah5"
        ])).ToList();
        await Assert.That(values.Count).IsEqualTo(5);
        await Assert.That(values.Count(value => value.HasValue)).IsEqualTo(1);
    }
        
    [Test]
    [Repeat(2)]
    public async Task SetGetDeleteSingleKey()
    {
        await Client.StringSetAsync("SingleKey", "123");
        var redisValue = await Client.StringGetAsync("SingleKey");
        await Assert.That(redisValue.Value).IsEqualTo("123");
            
        await Client.DeleteKeyAsync("SingleKey");
        redisValue = await Client.StringGetAsync("SingleKey");
        await Assert.That(redisValue.HasValue).IsEqualTo(false);
    }

    [Test]
    [Repeat(2)]
    public async Task SetGetSingleKeyWithTtl()
    {
        await Client.StringSetAsync("SingleKeyWithTtl", "123", 30);
        var redisValue = await Client.StringGetAsync("SingleKeyWithTtl");
        await Assert.That(redisValue.Value).IsEqualTo("123");
    }

    [Test]
    [Repeat(2)]
    public async Task SetMultipleTtl()
    {
        var client = Client;
        await client.StringSetAsync(new List<RedisKeyValue>
            {
                new("BlahTTL1", "Blah1"),
                new("BlahTTL2", "Blah2"),
                new("BlahTTL3", "Blah3"),
                new("BlahTTL4", "Blah4"),
                new("BlahTTL5", "Blah5")
            },
            120);

        var ttl = await client.TimeToLiveAsync("BlahTTL1");
        await Assert.That(ttl).IsPositive().And.IsLessThanOrEqualTo(125);
    }
        
    [Test]
    [Repeat(2)]
    public async Task MultipleExpire()
    {
        var client = Client;
        await client.StringSetAsync(new List<RedisKeyValue>
            {
                new("BlahExpire1", "Blah1"),
                new("BlahExpire2", "Blah2"),
                new("BlahExpire3", "Blah3"),
                new("BlahExpire4", "Blah4"),
                new("BlahExpire5", "Blah5")
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
        await Assert.That(ttl).IsPositive().And.IsLessThanOrEqualTo(125);
    }

    [Test]
    [Repeat(2)]
    public async Task SetGetMultipleKey()
    {
        var keyValues = new List<RedisKeyValue>
        {
            new("MultiKey1", "1"),
            new("MultiKey2", "2"),
            new("MultiKey3", "3")
        };
            
        await Client.StringSetAsync(keyValues);
        var redisValues = (await Client.StringGetAsync(["MultiKey1", "MultiKey2", "MultiKey3"])).ToList();
        await Assert.That(redisValues.Count).IsEqualTo(3);
        await Assert.That(redisValues[0].Value).IsEqualTo("1");
        await Assert.That(redisValues[1].Value).IsEqualTo("2");
        await Assert.That(redisValues[2].Value).IsEqualTo("3");
    }

    [Test]
    [Repeat(2)]
    public async Task Pipelining_Multiple_Sets()
    {
        var tomLonghurstRedisClient = Client;
            
        var keys = new[]
        {
            "Pipeline1", "Pipeline2", "Pipeline3", "Pipeline4", "Pipeline5", "Pipeline6", "Pipeline7", "Pipeline8"
        };
        var results = keys.Select(key => tomLonghurstRedisClient.StringSetAsync(key, "123", 30));
            
        await Task.WhenAll(results);

        foreach (var key in keys)
        {
            var value = await Client.StringGetAsync(key);
            await Assert.That(value.Value).IsEqualTo("123");
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
    public async Task SetGetMultipleKeyWithTtl()
    {
        var keyValues = new List<RedisKeyValue>
        {
            new("MultiKeyWithTtl1", "1"),
            new("MultiKeyWithTtl2", "2"),
            new("MultiKeyWithTtl3", "3")
        };
            
        await Client.StringSetAsync(keyValues, 120);
        var redisValues = (await Client.StringGetAsync(["MultiKeyWithTtl1", "MultiKeyWithTtl2", "MultiKeyWithTtl3"
        ])).ToList();
        await Assert.That(redisValues.Count).IsEqualTo(3);
        await Assert.That(redisValues[0].Value).IsEqualTo("1");
        await Assert.That(redisValues[1].Value).IsEqualTo("2");
        await Assert.That(redisValues[2].Value).IsEqualTo("3");
            
        var ttl = await Client.TimeToLiveAsync("MultiKeyWithTtl1");
        await Assert.That(ttl).IsLessThanOrEqualTo(120).And.IsPositive();
            
        ttl = await Client.TimeToLiveAsync("MultiKeyWithTtl2");
        await Assert.That(ttl).IsLessThanOrEqualTo(120).And.IsPositive();
            
        ttl = await Client.TimeToLiveAsync("MultiKeyWithTtl3");
        await Assert.That(ttl).IsLessThanOrEqualTo(120).And.IsPositive();
    }

    [Test]
    [Repeat(2)]
    public async Task KeyExists()
    {
        await Client.StringSetAsync("KeyExistsCheck", "123", 30);
        var exists = await Client.KeyExistsAsync("KeyExistsCheck");
        var doesntExist = await Client.KeyExistsAsync("KeyDoesntExistsCheck");
            
        await Assert.That(exists).IsEqualTo(true);
        await Assert.That(doesntExist).IsEqualTo(false);
    }
        
    [Test, Skip("No Cluster Support on Redis Server")]
    public async Task ClusterInfo()
    {
        var response = await Client.Cluster.ClusterInfoAsync();
        var firstLine = response.Split("\n").First();
        await Assert.That(firstLine).IsEqualTo("cluster_state:ok");
    }

    // Needs Access to Socket (which is private) to Close it
    [Test]
    [Repeat(2)]
    public async Task Disconnected()
    {
        var client = _redisManager.GetRedisClient();
        await client.StringSetAsync("DisconnectTest", "123", 120);
        var redisValue = await client.StringGetAsync("DisconnectTest");
        await Assert.That(redisValue.Value).IsEqualTo("123");
            
        client.Socket.Close();

        await Task.Delay(1000);
            
        var result = await client.StringGetAsync("DisconnectTest");
            
        await Assert.That(result.Value).IsEqualTo("123");
    }

    [Test]
    public async Task GetKey()
    {
        var result =
            await Client.StringGetAsync(
                "SummaryProduct_V3_1022864315_1724593328_COM_GBP_UK_en-GB_");
            
        Console.Write(result);
    }
        
    [Test]
    public async Task GetKeys()
    {
        var keys = GenerateMassKeys();

        var client1 = _redisManager.GetRedisClient();
        var client2 = _redisManager.GetRedisClient();
        var client3 = _redisManager.GetRedisClient();
            
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
                [
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
                ]
            );
        }

        return list;
    }

    [Arguments("IncrKey")]
    [Test]
    [Repeat(2)]
    public async Task Incr(string key)
    {
        await Client.DeleteKeyAsync(key);
            
        var one = await Client.IncrementAsync(key);
        await Assert.That(one).IsEqualTo(1);
            
        var three = await Client.IncrementByAsync(key, 2);
        await Assert.That(three).IsEqualTo(3);
            
        var threeAndAHalf = await Client.IncrementByAsync(key, 0.5f);
        await Assert.That(threeAndAHalf).IsEqualTo(3.5f);
            
        var four = await Client.IncrementByAsync(key, 0.5f);
        await Assert.That(four).IsEqualTo(4f);
            
        var five = await Client.IncrementAsync(key);
        await Assert.That(five).IsEqualTo(5);
            
        await Client.ExpireAsync(key, 120);
    }

    [Arguments("IncrKey")]
    [Test]
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
        var info = await Client.Server.Info();
    }
        
    [Test]
    [Repeat(2)]
    public async Task DBSize()
    {
        var tomLonghurstRedisClient = Client;
        var dbSize = Client.Server.DbSize();
    }

    [Test]
    [Arguments("DecrKey")]
    [Repeat(2)]
    public async Task Decr(string key)
    {
        await Incr(key);

        var four = await Client.DecrementAsync(key);
        await Assert.That(four).IsEqualTo(4);
            
        var two = await Client.DecrementByAsync(key, 2);
        await Assert.That(two).IsEqualTo(2);
            
        var one = await Client.DecrementAsync(key);
        await Assert.That(one).IsEqualTo(1);
    }

    [Test]
    [Repeat(2)]
    public async Task Expire()
    {
        await Client.StringSetAsync("ExpireKey", "123");
        var ttl = await Client.TimeToLiveAsync("ExpireKey");
        await Assert.That(ttl).IsEqualTo(-1);
            
        await Client.ExpireAsync("ExpireKey", 30);
        ttl = await Client.TimeToLiveAsync("ExpireKey");
        await Assert.That(ttl).IsLessThanOrEqualTo(30);
            
        await Client.PersistAsync("ExpireKey");
        ttl = await Client.TimeToLiveAsync("ExpireKey");
        await Assert.That(ttl).IsEqualTo(-1);
            
        await Client.ExpireAsync("ExpireKey", 30);
        ttl = await Client.TimeToLiveAsync("ExpireKey");
        await Assert.That(ttl).IsLessThanOrEqualTo(30);
    }
        
    [Test]
    [Repeat(2)]
    public async Task ExpireAt()
    {
        await Client.StringSetAsync("ExpireKeyDateTime", "123");
        await Client.ExpireAtAsync("ExpireKeyDateTime", DateTimeOffset.Now.AddSeconds(30));
        var ttl = await Client.TimeToLiveAsync("ExpireKeyDateTime");
        await Assert.That(ttl).IsLessThanOrEqualTo(33);
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