using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using Domain.Entities;
using Application.Interfaces;
using Application.DTOs;
using MarketplaceParsers;
using System.Globalization;

namespace Infrastructure.Parsers
{
    public abstract class BaseSeleniumParser : IProductParser
    {
        protected readonly WebDriverPool _driverPool;
        protected readonly ParserConfig _config;

        protected BaseSeleniumParser(WebDriverPool driverPool, string configPath)
        {
            _driverPool = driverPool;
            _config = ParserConfig.LoadConfig(configPath);
        }

        public async Task<List<Product>> GetCheapestPricesAsync(string productName)
        {
            Console.WriteLine($"[INFO] Запуск парсинга {_config.MarketUrl} для: {productName}");
            var driver = await _driverPool.GetDriverAsync();
            try
            {
                driver.Navigate().GoToUrl(_config.MarketUrl);
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Открыли главную страницу.");

                var searchBox = SeleniumHelper.WaitForElement(By.XPath(_config.SearchBoxXPath), driver);
                await Task.Delay(_config.WaitingBeforeSearching);
                searchBox.SendKeys(productName);
                searchBox.SendKeys(Keys.Enter);
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Ввел запрос и нажал Enter");

                SeleniumHelper.WaitForElements(By.XPath(_config.ProductElementsXPath), driver);
                await Task.Delay(_config.WaitingForAction);
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Найдены товары, выполняем сортировку...");

                PerformSorting(driver);
                await Task.Delay(_config.WaitingForAction);

                // Прокрутка страницы для загрузки всех товаров
                for (int i = 0; i < _config.MaxScrollAttempts; i++)
                {
                    driver.ExecuteScript("window.scrollBy(0, 1000);");
                    await Task.Delay(_config.ScrollDelayMs);
                }

                var productElements = driver.FindElements(By.XPath(_config.ProductElementsXPath)).ToList();
                Console.WriteLine($"[INFO] {_config.MarketUrl}: Найдено {productElements.Count} товаров");

                if (productElements.Count == 0)
                    return new List<Product>();

                return ParseProductList(productElements);
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

        // Конвертация ProductDto -> Product
        protected Product ConvertToProduct(ProductDto dto)
        {
            return new Product
            {
                Marketplace = dto.Marketplace,
                Name = dto.Name,
                Brand = dto.Brand,
                Price = dto.Price,
                Url = dto.Url,
                Image = dto.ImageUrl,

                // Исправляем рейтинг: пытаемся спарсить, если не удается - ставим 0
                Rating = decimal.TryParse(dto.Rating.Replace(",", "."),
                                          NumberStyles.AllowDecimalPoint,
                                          CultureInfo.InvariantCulture, out var rating) ? rating : 0,

                // Исправляем отзывы: извлекаем только число из строки ("123 отзывов" → 123)
                Reviews = int.TryParse(Regex.Match(dto.ReviewsCount, @"\d+").Value, out var reviews) ? reviews : 0
            };
        }


        protected abstract void PerformSorting(IWebDriver driver);
        protected abstract ProductDto ParseProductCard(IWebElement card);

        // Обновленный метод обработки списка товаров
        protected virtual List<Product> ParseProductList(List<IWebElement> productElements)
        {
            var products = new List<ProductDto>();

            foreach (var element in productElements)
            {
                try
                {
                    var productDto = ParseProductCard(element);
                    if (productDto != null)
                    {
                        products.Add(productDto);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка обработки товара на {_config.MarketUrl}: {ex.Message}");
                }
            }

            // Оставляем самые дешевые товары в количестве: _config.NumberOfProducts
            var sortedProducts = products.OrderBy(p => p.Price).Take(_config.NumberOfProducts).ToList();
            Console.WriteLine($"[INFO] {_config.MarketUrl}: После фильтрации осталось {sortedProducts.Count} товаров.");

            // Конвертируем ProductDto -> Product перед возвратом
            return sortedProducts.Select(ConvertToProduct).Where(p => p != null).ToList();
        }
    }
}
