using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace RedisClientTest
{
    public class TestUtilities
    {
        public static TimeSpan Time(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Console.WriteLine($"Time Taken: {stopwatch.Elapsed.ToString()}");
            return stopwatch.Elapsed;
        }
        
        public static async Task<TimeSpan> TimeAsync(Func<Task> action)
        {
            var stopwatch = Stopwatch.StartNew();
            await action();
            stopwatch.Stop();
            Console.WriteLine($"Time Taken: {stopwatch.Elapsed.ToString()}");
            return stopwatch.Elapsed;
        }
    }
}