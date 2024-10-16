namespace TomLonghurst.AsyncRedisClient.Extensions;

public static class TaskExtensions
{
    public static bool IsCompletedSuccessfully(this Task task) =>
        task.IsCompleted && !(task.IsCanceled || task.IsFaulted);

    public static async Task<T?> WhenAny<T>(this IEnumerable<Task<T>> tasks, Predicate<T> condition)
    {
        var tasksList = tasks.ToList();
        
        while (tasksList.Count > 0)
        {
            var task = await Task.WhenAny(tasksList);
            var t = await task;

            if (condition(t))
            {
                return t;
            }
                
            tasksList.Remove(task);
        }

        return default;
    }
}