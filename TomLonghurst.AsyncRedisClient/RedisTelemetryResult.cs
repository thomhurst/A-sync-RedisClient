namespace TomLonghurst.AsyncRedisClient;

public class RedisTelemetryResult
{
    private readonly string _command;
    private readonly TimeSpan _duration;

    // TODO - More metrics here
        
    internal RedisTelemetryResult(string command, TimeSpan duration)
    {
        _command = command;
        _duration = duration;
    }
}