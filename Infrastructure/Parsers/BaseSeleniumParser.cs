using System.Globalization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using Domain.Entities;
using Domain.DTOs;
using Application.Interfaces;
using MarketplaceParsers;

namespace Infrastructure.Parsers
{
    public abstract class BaseSeleniumParser : IProductParser
    {
        protected readonly WebDriverPool _driverPool;
        protected readonly ParserConfig _config;
        protected readonly int _CountForBestPrice = 5;
        protected readonly decimal _bestPrice = 0.75m; // 0.75m  если захочется сортировать

        private static readonly Regex _digitRegex = new(@"\d+", RegexOptions.Compiled);

        protected BaseSeleniumParser(WebDriverPool driverPool, string configPath)
        {
            _driverPool = driverPool;
            _config = ParserConfig.LoadConfig(configPath);
        }

        public async Task<List<Product>> GetCheapestPricesAsync(string productName)
        {
            Console.WriteLine($"[INFO] Запуск парсинга {_config.MarketUrl} для: {productName}");
            var driver = await _driverPool.GetDriverAsync();
            if (driver == null)
            {
                Console.WriteLine($"[ERROR] Не удалось получить драйвер для {_config.MarketUrl}");
                return new List<Product>();
            }

            using var cts = new CancellationTokenSource(_config.ParserTimeoutMs);
            var token = cts.Token;

            try
            {
                token.ThrowIfCancellationRequested();

                driver.Navigate().GoToUrl(_config.MarketUrl);
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Открыли главную страницу.");
                Console.WriteLine(driver.PageSource);

                var searchBox = SeleniumHelper.WaitForClickableElement(By.XPath(_config.SearchBoxXPath), driver);

                if (_config.WaitCardsBeforeSearch)
                    SeleniumHelper.WaitForElements(By.XPath(_config.ProductElementsXPath), driver);

                await Task.Delay(_config.WaitingBeforeSearching, token);
                searchBox.SendKeys(productName);
                searchBox.SendKeys(Keys.Enter);
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Ввел запрос и нажал Enter");

                SeleniumHelper.WaitForElements(By.XPath(_config.ProductElementsXPath), driver);
                await Task.Delay(_config.WaitingForAction, token);
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Найдены товары, выполняем сортировку...");

                PerformSorting(driver);
                await Task.Delay(_config.WaitingForAction, token);

                for (int i = 0; i < _config.MaxScrollAttempts; i++)
                {
                    driver.ExecuteScript("window.scrollBy(0, 1200);");
                    await Task.Delay(_config.ScrollDelayMs, token);
                }

                var productElements = driver.FindElements(By.XPath(_config.ProductElementsXPath));
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Найдено {productElements.Count} товаров");

                if (!productElements.Any())
                    return new List<Product>();

                return ParseProductList(productElements);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[WARNING] {_config.MarketUrl}: Парсинг прерван по таймауту ({_config.ParserTimeoutMs} мс)");
                return new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка парсинга {_config.MarketUrl}: {ex.Message}");
                return new List<Product>();
            }
            finally
            {
                _driverPool.ReleaseDriver(driver);
            }
        }

        protected virtual List<Product> ParseProductList(IEnumerable<IWebElement> productElements)
        {
            var products = new List<ProductDto>();
            var seenUrls = new HashSet<string>();

            foreach (var element in productElements)
            {
                try
                {
                    var productDto = ParseProductCard(element);
                    if (productDto == null) continue;

                    var normalizedUrl = NormalizeUrl(productDto.Url);
                    if (seenUrls.Contains(normalizedUrl)) continue;

                    seenUrls.Add(normalizedUrl);
                    productDto.Url = normalizedUrl;
                    products.Add(productDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка обработки товара на {_config.MarketUrl}: {ex.Message}");
                }
            }


            //======= берём первые {_CountForBestPrice} товаров как есть
            var topFive = products.Take(_CountForBestPrice).ToList();
            if (topFive.Count == 0)
            {
                Console.WriteLine($"[WARNING] {_config.MarketUrl}: Недостаточно данных для медианы");
                return new List<Product>();
            }

            var median = GetMedian(topFive.Select(p => p.Price).ToList());
            Console.WriteLine($"[INFO] {_config.MarketUrl}: Медианная цена из первых {_CountForBestPrice}: {median}");

            var filtered = products
                .Where(p => p.Price >= _bestPrice * median)
                .OrderBy(p => p.Price)
                .Take(_config.NumberOfProducts)
                .ToList();
            //=======



            var sorted = filtered;
            var result = new List<Product>();

            foreach (var dto in sorted)
            {
                var product = ConvertToProduct(dto);
                if (product != null)
                    result.Add(product);
            }

            Console.WriteLine($"[INFO] {_config.MarketUrl}: После фильтрации осталось {result.Count} товаров.");
            return result;
        }


        //======= метод расчёта медианы
        private decimal GetMedian(List<decimal> numbers)
        {
            var sorted = numbers.OrderBy(n => n).ToList();
            int count = sorted.Count;
            if (count == 0) return 0;

            return count % 2 == 1
                ? sorted[count / 2]
                : (sorted[(count - 1) / 2] + sorted[count / 2]) / 2;
        }
        //======= 


        protected Product ConvertToProduct(ProductDto dto)
        {
            decimal parsedRating = 0;

            if (decimal.TryParse(dto.Rating.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var rating))
            {
                parsedRating = Math.Min(rating, 5.0m); // Ограничиваем рейтинг максимум до 5.0
            }

            return new Product
            {
                Marketplace = dto.Marketplace,
                Name = dto.Name,
                Brand = dto.Brand,
                Price = dto.Price,
                Url = dto.Url,
                Image = dto.ImageUrl,
                Rating = parsedRating,
                Reviews = int.TryParse(_digitRegex.Match(dto.ReviewsCount).Value, out var reviews) ? reviews : 0
            };
        }

        protected string NormalizeUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.GetLeftPart(UriPartial.Path);
            }
            catch
            {
                return url;
            }
        }

        protected abstract void PerformSorting(IWebDriver driver);
        protected abstract ProductDto ParseProductCard(IWebElement card);
    }
}