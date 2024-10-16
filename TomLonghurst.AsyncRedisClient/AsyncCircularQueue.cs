namespace TomLonghurst.AsyncRedisClient;

public class CircularQueue<T>
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
}