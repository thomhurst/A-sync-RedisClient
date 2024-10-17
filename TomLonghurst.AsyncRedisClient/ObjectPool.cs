using System.Collections.Concurrent;

namespace TomLonghurst.AsyncRedisClient;

public class ObjectPool<T>(Func<T> objectGenerator)
{
    private readonly ConcurrentBag<T> _objects = [];
    private readonly Func<T> _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));

    public T Get()
    {
        return _objects.TryTake(out var item) ? item : _objectGenerator();
    }

    public void Return(T item) => _objects.Add(item);
}