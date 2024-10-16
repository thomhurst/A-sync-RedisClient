namespace TomLonghurst.AsyncRedisClient;

public class CircularQueue<T> : IAsyncDisposable
{
    private readonly Func<Task<T>> _objectGenerator;
    private readonly int _maxPoolSize;
    private T[]? _clients;
    private readonly Lock _lock = new();

    private int _index = -1;

    public CircularQueue(Func<Task<T>> objectGenerator, int maxPoolSize)
    {
        _maxPoolSize = maxPoolSize;
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
    }

    public async Task InitializeAsync()
    {
        var clients = Enumerable.Range(0, _maxPoolSize)
            .Select(_ => _objectGenerator()).
            ToArray();

        _clients = await Task.WhenAll(clients);
    }

    public T Get()
    {
        return _clients![GetIndex()];
    }

    private int GetIndex()
    {
        lock (_lock)
        {
            var index = ++_index;

            if (index == _maxPoolSize - 1)
            {
                return _index = 0;
            }

            return index;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.WhenAll(_clients!.Select(DisposeAsync));
    }

    private async Task DisposeAsync(T t)
    {
        if (t is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }

        if (t is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}