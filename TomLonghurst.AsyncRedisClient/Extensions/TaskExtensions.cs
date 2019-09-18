using System.Threading.Tasks;

namespace TomLonghurst.AsyncRedisClient.Extensions
{
    public static class TaskExtensions
    {
        public static bool IsCompletedSuccessfully(this Task task) =>
            task.IsCompleted && !(task.IsCanceled || task.IsFaulted);
    }
}