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
            Value = $"{StringConstants.VALUE_DELIMITER} {value.Replace(StringConstants.NEW_LINE, StringConstants.ENCODED_NEW_LINE)} {StringConstants.VALUE_DELIMITER}";
        }
    }
}