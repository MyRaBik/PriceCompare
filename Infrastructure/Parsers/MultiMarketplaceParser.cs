using System.Diagnostics;
using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Parsers
{
    public class MultiMarketplaceParser : IProductParser
    {
        private readonly IEnumerable<IProductParser> _parsers;
        private readonly GlobalParsingQueue _queue;

        public MultiMarketplaceParser(IEnumerable<IProductParser> parsers, GlobalParsingQueue queue)
        {
            _parsers = parsers;
            _queue = queue;
        }

        public async Task<List<Product>> GetCheapestPricesAsync(string productName)
        {
            Console.WriteLine($"\n[INFO] Начало парсинга для товара: {productName}");
            var totalStopwatch = Stopwatch.StartNew();

            var tasks = _parsers.Select(parser =>
            {
                return _queue.Enqueue(async () =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    var result = await parser.GetCheapestPricesAsync(productName);
                    stopwatch.Stop();
                    Console.WriteLine($"[INFO] {parser.GetType().Name} завершил за {stopwatch.ElapsedMilliseconds} мс");
                    return result;
                });
            });

            var results = await Task.WhenAll(tasks);
            totalStopwatch.Stop();

            Console.WriteLine($"\n[INFO] Парсинг завершен. Общее время: {totalStopwatch.ElapsedMilliseconds} мс");
            return results.SelectMany(r => r).ToList();
        }
    }
}
