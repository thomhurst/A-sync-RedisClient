namespace TomLonghurst.AsyncRedisClient.Models.RequestModels;

public struct RedisKeyFieldValue
{
    public string Key { get; }
    public string Field { get; }
    public string Value { get; }

    public RedisKeyFieldValue(string key, string field, string value)
    {
        Key = key;
        Field = field;
        Value = value;
    }
}