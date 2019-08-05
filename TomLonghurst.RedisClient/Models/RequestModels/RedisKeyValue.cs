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
            if (value?.Contains(StringConstants.NEW_LINE) == true)
            {
                Value = value.Replace(StringConstants.NEW_LINE, StringConstants.ENCODED_NEW_LINE);
            }
            else
            {
                Value = value;
            }
        }
    }
}