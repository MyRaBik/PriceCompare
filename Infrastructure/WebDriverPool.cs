using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium.Chrome;

namespace MarketplaceParsers
{
    public class WebDriverPool : IDisposable
    {
        private readonly ConcurrentQueue<ChromeDriver> _drivers = new();
        private readonly ConcurrentQueue<TaskCompletionSource<ChromeDriver>> _pendingQueue = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly object _lock = new();
        private bool _disposed = false;

        public WebDriverPool(int poolSize)
        {
            _semaphore = new SemaphoreSlim(poolSize, poolSize);
        }

        public Task<ChromeDriver> GetDriverAsync()
        {
            var tcs = new TaskCompletionSource<ChromeDriver>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_lock)
            {
                _pendingQueue.Enqueue(tcs);
                TryDequeueNext(); // Попытаться обработать сразу, если есть доступный слот
            }

            return tcs.Task;
        }

        private void TryDequeueNext()
        {
            if (_semaphore.CurrentCount == 0) return;

            while (_pendingQueue.TryDequeue(out var tcs))
            {
                _ = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync();

                    ChromeDriver driver;

                    lock (_lock)
                    {
                        if (!_drivers.TryDequeue(out driver) || !IsDriverValid(driver))
                        {
                            driver?.Quit();
                            driver?.Dispose();
                            driver = CreateNewDriver();
                            Console.WriteLine($"[INFO] Создан новый ChromeDriver");
                        }
                        else
                        {
                            Console.WriteLine($"[INFO] Выдан ChromeDriver из пула | Доступно: {_semaphore.CurrentCount}");
                        }
                    }

                    tcs.SetResult(driver);
                });

                break; // Только одного запускаем за раз
            }
        }

        public void ReleaseDriver(ChromeDriver driver)
        {
            if (IsDriverValid(driver))
            {
                _drivers.Enqueue(driver);
                Console.WriteLine($"[INFO] Возвращен рабочий ChromeDriver | Доступно: {_semaphore.CurrentCount}");
            }
            else
            {
                try
                {
                    driver.Quit();
                    driver.Dispose();
                    Console.WriteLine($"[WARNING] Уничтожен нерабочий ChromeDriver при возврате");
                }
                catch
                {
                    // Игнорируем ошибки при закрытии
                }
            }

            _semaphore.Release();

            lock (_lock)
            {
                TryDequeueNext(); // Запустить следующий запрос из очереди
            }
        }

        private bool IsDriverValid(ChromeDriver driver)
        {
            try
            {
                return driver != null && driver.SessionId != null && driver.WindowHandles.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private ChromeDriver CreateNewDriver()
        {
            return new ChromeDriver(SeleniumHelper.GetChromeOptions());
        }

        public void Dispose()
        {
            if (_disposed) return;

            while (_drivers.TryDequeue(out var driver))
            {
                try
                {
                    driver.Quit();
                    driver.Dispose();
                }
                catch
                {
                    // Игнорируем ошибки
                }
            }

            _semaphore.Dispose();
            _disposed = true;
        }
    }

    public class WebDriverPoolDisposer : IHostedService
    {
        private readonly WebDriverPool _pool;

        public WebDriverPoolDisposer(WebDriverPool pool)
        {
            _pool = pool;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _pool.Dispose();
            return Task.CompletedTask;
        }
    }
}
