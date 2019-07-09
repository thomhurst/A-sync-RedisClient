using System;

namespace TomLonghurst.RedisClient.Models
{
    public class Pong
    {
        public TimeSpan TimeTaken { get; }
        public string Message { get; }

        public bool IsSuccessful => Message == "PONG";

        internal Pong(TimeSpan timeTaken, string message)
        {
            TimeTaken = timeTaken;
            Message = message;
        }
    }
}