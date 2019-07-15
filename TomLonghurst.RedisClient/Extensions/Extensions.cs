using System;

namespace TomLonghurst.RedisClient.Extensions
{
    public static class Extensions
    {
        public static T Also<T>(this T t, Action<T> action)
        {
            action.Invoke(t);
            return t;
        }
    }
}