using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Parsers
{
    public class MultiMarketplaceParser : IProductParser
    {
        private readonly IEnumerable<IProductParser> _parsers;
        private readonly SemaphoreSlim _semaphore;
        private readonly int _timeoutMilliseconds = 60000; // 60 секунд таймаут

        public MultiMarketplaceParser(IEnumerable<IProductParser> parsers)
        {
            _parsers = parsers;
            _semaphore = new SemaphoreSlim(3);
        }

        public async Task<List<Product>> GetCheapestPricesAsync(string productName)
        {
            Console.WriteLine($"\n[INFO] Начало парсинга для товара: {productName}");
            var totalStopwatch = Stopwatch.StartNew(); // Замер общего времени

            var tasks = _parsers.Select(async parser =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    using var cts = new CancellationTokenSource(_timeoutMilliseconds);
                    var task = parser.GetCheapestPricesAsync(productName);

                    var stopwatch = Stopwatch.StartNew(); // Замер времени для конкретного парсера

                    if (await Task.WhenAny(task, Task.Delay(_timeoutMilliseconds, cts.Token)) == task)
                    {
                        cts.Cancel(); // Отменяем таймер, если парсер завершился раньше
                        var result = await task;
                        stopwatch.Stop();
                        Console.WriteLine($"[INFO] {parser.GetType().Name} завершил работу за {stopwatch.ElapsedMilliseconds} мс, товаров найдено: {result.Count}");
                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] {parser.GetType().Name} превысил лимит {(_timeoutMilliseconds / 1000)} секунд и был остановлен.");
                        return new List<Product>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка в {parser.GetType().Name}: {ex.Message}");
                    return new List<Product>();
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            totalStopwatch.Stop(); // Остановка таймера общего выполнения

            Console.WriteLine($"\n[INFO] Парсинг завершен. Общее время выполнения: {totalStopwatch.ElapsedMilliseconds} мс");
            return results.SelectMany(r => r).ToList();
        }
    }
}
