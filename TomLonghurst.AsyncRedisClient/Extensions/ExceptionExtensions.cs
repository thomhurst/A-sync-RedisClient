namespace TomLonghurst.AsyncRedisClient.Extensions
{
    internal static class ExceptionExtensions
    {
        internal static bool IsSameOrSubclassOf(this Exception exception, Type typeToCheckAgainst)
        {
            var exceptionType = exception.GetType();
            return exceptionType == typeToCheckAgainst || exceptionType.IsSubclassOf(typeToCheckAgainst);
        }
    }
}