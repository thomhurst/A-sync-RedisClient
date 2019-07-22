using TomLonghurst.RedisClient.Constants;

namespace TomLonghurst.RedisClient.Models
{
    public class RedisValue<T>
    {
        public T Value { get; protected set; }
        public bool IsNull => Equals(Value,default(T));
        public bool HasValue => !IsNull;

        internal RedisValue(T value)
        {
            Value = value;
        }
    }

    public class StringRedisValue : RedisValue<string>
    {
        internal StringRedisValue(string value) : base(value)
        {
            Value = value?.Replace(CharacterConstants.ENCODED_NEW_LINE, CharacterConstants.NEW_LINE);
        }
    }
}