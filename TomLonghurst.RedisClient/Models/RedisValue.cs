using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Models
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
            if (value?.Contains(StringConstants.ENCODED_NEW_LINE) == true)
            {
                Value = value.Replace(StringConstants.ENCODED_NEW_LINE, StringConstants.NEW_LINE);
            }
            else
            {
                Value = value;
            }
        }
    }
}