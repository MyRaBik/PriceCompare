using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace MarketplaceParsers
{
    public class WebDriverPool : IDisposable
    {
        private readonly ConcurrentQueue<ChromeDriver> _drivers = new();
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public WebDriverPool(int poolSize)
        {
            _semaphore = new SemaphoreSlim(poolSize, poolSize);
        }

        public async Task<ChromeDriver> GetDriverAsync()
        {
            await _semaphore.WaitAsync();

            if (!_drivers.TryDequeue(out var driver))
            {
                driver = new ChromeDriver(SeleniumHelper.GetChromeOptions()); // Ленивая инициализация
                Console.WriteLine($"[INFO] Создан новый ChromeDriver");
            }
            else
            {
                Console.WriteLine($"[INFO] Выдан ChromeDriver из пула | Доступно: {_semaphore.CurrentCount}");
            }

            return driver;
        }

        public void ReleaseDriver(ChromeDriver driver)
        {
            _drivers.Enqueue(driver);
            _semaphore.Release();
            Console.WriteLine($"[INFO] Возвращен ChromeDriver | Доступно: {_semaphore.CurrentCount}");
        }

        public void Dispose()
        {
            if (_disposed) return;

            while (_drivers.TryDequeue(out var driver))
            {
                driver.Quit();
                driver.Dispose();
            }

            _semaphore.Dispose();
            _disposed = true;
        }
    }
}
