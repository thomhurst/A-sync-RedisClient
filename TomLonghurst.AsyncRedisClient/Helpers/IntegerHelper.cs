using System.Runtime.CompilerServices;

namespace TomLonghurst.AsyncRedisClient.Helpers;

public static class IntegerHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetDigitCount(this int number)
    {
        return (int) Math.Floor(Math.Log10(number) + 1);
    }
}