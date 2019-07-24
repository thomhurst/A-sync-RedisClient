using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Models.RequestModels
{
    public class RedisKeyFieldValue
    {
        public string Key { get; }
        public string Field { get; }
        public string Value { get; }

        public RedisKeyFieldValue(string key, string field, string value)
        {
            Key = key;
            Field = field;
            Value = $"{StringConstants.VALUE_DELIMITER} {value.Replace(StringConstants.NEW_LINE, StringConstants.ENCODED_NEW_LINE)}";
        }
    }
}