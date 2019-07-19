using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Models.RequestModels
{
    public class RedisKeyValue
    {
        public string Key { get; }
        public string Value { get; }

        public RedisKeyValue(string key, string value)
        {
            Key = key;
            Value = $"{CharacterConstants.VALUE_DELIMITER} {value} {CharacterConstants.VALUE_DELIMITER}";
        }
    }
}