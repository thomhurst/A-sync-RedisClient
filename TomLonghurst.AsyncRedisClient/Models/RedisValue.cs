namespace TomLonghurst.AsyncRedisClient.Models
{
    public struct StringRedisValue
    {
        public string Value { get; }
        public bool HasValue => !string.IsNullOrEmpty(Value);

        internal StringRedisValue(string value)
        {
            Value = value;
        }
    }
}