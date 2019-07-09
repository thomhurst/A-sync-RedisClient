namespace TomLonghurst.RedisClient.Models
{
    public class RedisValue<T>
    {
        public T Value { get; }
        public bool IsNull => Value == null;
        public bool HasValue => !IsNull;

        internal RedisValue(T value)
        {
            Value = value;
        }
    }
}