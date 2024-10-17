using System.Collections.Concurrent;

namespace TomLonghurst.AsyncRedisClient;

public class AsyncObjectPool<T>(Func<ValueTask<T>> objectGenerator)
{
    private readonly ConcurrentBag<T> _objects = [];
    private readonly Func<ValueTask<T>> _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));

    public async ValueTask<T> Get()
    {
        if (_objects.TryTake(out var item))
        {
            return item;
        }
        
        return await _objectGenerator();
    }

    public void Return(T item) => _objects.Add(item);
}