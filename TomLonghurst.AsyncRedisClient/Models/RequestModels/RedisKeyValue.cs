namespace TomLonghurst.AsyncRedisClient.Models.RequestModels;

public struct RedisKeyValue
{
    public string Key { get; }
    public string Value { get; }

    public RedisKeyValue(string key, string value)
    {
        Key = key;
        Value = value;
    }
}