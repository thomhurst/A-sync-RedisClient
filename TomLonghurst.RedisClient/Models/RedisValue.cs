using System.Runtime.InteropServices;

namespace TomLonghurst.RedisClient.Models
{
    public class RedisValue<T>
    {
        public string Key { get; internal set; }
        public T Value { get; }
        public bool IsNull => Equals(Value,default(T));
        public bool HasValue => !IsNull;

        internal RedisValue(T value)
        {
            Value = value;
        }
    }
}