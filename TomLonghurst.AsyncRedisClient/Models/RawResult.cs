using System.Collections.Generic;

namespace TomLonghurst.AsyncRedisClient.Models
{
    public class RawResult
    {
        private readonly object _rawResult;

        internal RawResult(object rawResult)
        {
            _rawResult = rawResult;
        }

        public StringRedisValue GetAsWord()
        {
            return (StringRedisValue) _rawResult;
        }
        
        public StringRedisValue GetAsComplexString()
        {
            return (StringRedisValue) _rawResult;
        }
        
        public IEnumerable<StringRedisValue> GetAsArray()
        {
            return (IEnumerable<StringRedisValue>) _rawResult;
        }
        
        public int GetAsInteger()
        {
            return (int) _rawResult;
        }
        
        public float GetAsFloat()
        {
            return (float) _rawResult;
        }
    }
}