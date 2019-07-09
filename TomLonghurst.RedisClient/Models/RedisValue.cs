namespace TomLonghurst.RedisClient.Models
{
    public class RedisValue<T>
    {
        public T Value { get; }
        public bool IsNull => Equals(Value,default(T));
        public bool HasValue => !IsNull;

        internal RedisValue(T value)
        {
            Value = value;
        }
    }
}