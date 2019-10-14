using System;
using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public struct StringRedisValue
    {
        public string Value { get; }
        public bool HasValue => !string.IsNullOrEmpty(Value);

        internal StringRedisValue(string value)
        {
            if (value?.AsSpan().Contains(StringConstants.EncodedNewLine.AsSpan(), StringComparison.Ordinal) == true)
            {
                Value = value.Replace(StringConstants.EncodedNewLine, StringConstants.NewLine);
            }
            else
            {
                Value = value;
            }
        }
    }
}