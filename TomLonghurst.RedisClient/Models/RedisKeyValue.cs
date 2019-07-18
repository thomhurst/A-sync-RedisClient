using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Models
{
    public class RedisKeyValue
    {
        public string Key { get; }
        public string Value { get; }

        public RedisKeyValue(string key, string value)
        {
            Key = key;
            Value = $"{CharacterConstants.QUOTE_IDENTIFIER} {value} {CharacterConstants.QUOTE_IDENTIFIER}";
        }
    }
}