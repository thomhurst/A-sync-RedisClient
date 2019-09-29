using System;
using TomLonghurst.AsyncRedisClient.Constants;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public class RedisValue<T>
    {
        public T Value { get; protected set; }
        public virtual bool HasValue => !Equals(Value,default(T));

        internal RedisValue(T value)
        {
            Value = value;
        }
    }

    public class StringRedisValue : RedisValue<string>
    {
        public override bool HasValue => !string.IsNullOrEmpty(Value);

        internal StringRedisValue(string value) : base(value)
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