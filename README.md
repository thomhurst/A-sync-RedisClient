# A[sync]RedisClient

[![nuget](https://img.shields.io/nuget/v/TomLonghurst.AsyncRedisClient.svg)](https://www.nuget.org/packages/TomLonghurst.AsyncRedisClient/)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/b692f8fbb14142d3ad8f9ccb65d0889c)](https://app.codacy.com/app/thomhurst/A-sync-RedisClient?utm_source=github.com&utm_medium=referral&utm_content=thomhurst/A-sync-RedisClient&utm_campaign=Badge_Grade_Dashboard)

## Please note this is still in development

## Install

Install via Nuget > `Install-Package TomLonghurst.AsyncRedisClient`

## Usage

### Connect
Create a `RedisClientConfig` object:

```csharp
var config = new RedisClientConfig(Host, Port, Password) {
  Ssl = true, 
  Timeout = 5000
  };
```

Create a new `RedisClientManager` object:

```csharp
int poolSize = 5;
var redisManager = new RedisClientManager(config, poolSize);
```

Call `RedisClientManager.GetRedisClientAsync()`

```csharp
var client = await redisManager.GetRedisClientAsync();
```

#### Pool Size
Each Redis Client can only perform one operation at a time. Because it's usually very fast, one is enough for most applications.
However if your application takes heavy traffic, and you are seeing `RedisTimeoutException`s then consider upping the pool size. 

### Commands

#### Ping
```csharp
var ping = await client.Ping();
```

#### Set
```csharp
await _client.StringSetAsync("key", "123", AwaitOptions.FireAndForget);
```

#### Set with TimeToLive
```csharp
await _client.StringSetAsync("key", "123", 120, AwaitOptions.FireAndForget);
```

#### Multi Set
```csharp
var keyValues = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("key1", "1"),
                new KeyValuePair<string, string>("key2", "2"),
                new KeyValuePair<string, string>("key3", "3")
            };
await _client.StringSetAsync(keyValues, AwaitOptions.AwaitCompletion);
```
            
#### Get
```csharp
var value = await _client.StringGetAsync("key");
```

#### Multi Get
```csharp
var values = await _client.StringGetAsync(new [] { "key1", "key2" });
```

#### Delete
```csharp
await _client.DeleteKeyAsync("key", AwaitOptions.AwaitCompletion);
```

#### Multi Delete
```csharp
await _client.DeleteKeyAsync(new [] { "key1", "key2" }, AwaitOptions.AwaitCompletion);
```

#### Key Exists
```csharp
var exists = await _client.KeyExistsAsync("key");
```

### AwaitOptions
Any method taking an AwaitOptions parameter has two options:

#### AwaitCompletion
Wait for the operation to complete on the Redis server before resuming program execution

#### FireAndForget
Resume with program execution instantly and forget about checking the result

If you enjoy, please buy me a coffee :)

<a href="https://www.buymeacoffee.com/tomhurst" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>
