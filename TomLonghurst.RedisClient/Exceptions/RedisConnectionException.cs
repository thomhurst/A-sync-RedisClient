using System;

namespace TomLonghurst.RedisClient.Exceptions
{
    public class RedisConnectionException : RedisException
    {
        private readonly Exception _innerException;

        public override Exception GetBaseException()
        {
            return _innerException;
        }

        public override string Message => $"{_innerException.Message} - {_innerException.GetType().Name}\n{_innerException}";

        public RedisConnectionException(Exception innerException)
        {
            _innerException = innerException;
        }
    }
}