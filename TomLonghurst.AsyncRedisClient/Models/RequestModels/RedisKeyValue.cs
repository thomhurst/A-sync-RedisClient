using System;
using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Models.RequestModels
{
    public class RedisKeyValue
    {
        public string Key { get; }
        public string Value { get; }

        public RedisKeyValue(string key, string value)
        {
            Key = key;
            if (value?.AsSpan().Contains(StringConstants.NewLine.AsSpan(), StringComparison.Ordinal) == true)
            {
                Value = value.Replace(StringConstants.NewLine, StringConstants.EncodedNewLine);
            }
            else
            {
                Value = value;
            }
        }
    }
}