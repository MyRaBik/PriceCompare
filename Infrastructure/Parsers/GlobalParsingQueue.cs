using System.Collections.Concurrent;

namespace Infrastructure.Parsers
{
    public class GlobalParsingQueue
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<Func<Task>> _queue = new();
        private readonly object _lock = new();

        public GlobalParsingQueue(int maxConcurrent)
        {
            _semaphore = new SemaphoreSlim(maxConcurrent);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            var tcs = new TaskCompletionSource<T>();

            _queue.Enqueue(async () =>
            {
                try
                {
                    var result = await taskGenerator();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    _semaphore.Release();
                    _ = ProcessQueueAsync();
                }
            });

            await ProcessQueueAsync();
            return await tcs.Task;
        }

        private Task _processingTask = Task.CompletedTask;

        private Task ProcessQueueAsync()
        {
            lock (_lock)
            {
                if (_processingTask.Status == TaskStatus.Running)
                    return _processingTask;

                _processingTask = Task.Run(async () =>
                {
                    while (_queue.TryDequeue(out var task))
                    {
                        await _semaphore.WaitAsync();
                        _ = Task.Run(task);
                    }
                });

                return _processingTask;
            }
        }
    }
}
