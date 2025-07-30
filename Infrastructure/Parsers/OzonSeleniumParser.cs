using System.Globalization;
using Domain.DTOs;
using System.Text.RegularExpressions;
using MarketplaceParsers;
using OpenQA.Selenium;

namespace Infrastructure.Parsers
{
    public class OzonSeleniumParser : BaseSeleniumParser
    {
        public OzonSeleniumParser(WebDriverPool driverPool)  
            : base(driverPool, "configs/ozon_config.json") { } 

        protected override void PerformSorting(IWebDriver driver)
        {
            // Ozon НАДО ДОПИЛИТЬ (как яндекс)
        }

        protected override ProductDto ParseProductCard(IWebElement card)
        {
            try
            {
                var nameElement = SeleniumHelper.FindElementSafe(card, _config.NameXPath);
                var priceElement = SeleniumHelper.FindElementSafe(card, _config.PriceXPath);
                var imageElement = SeleniumHelper.FindElementSafe(card, _config.ImageXPath);
                var urlElement = SeleniumHelper.FindElementSafe(card, _config.UrlXPath);
                var brandElement = SeleniumHelper.FindElementSafe(card, _config.BrandXPath);
                var ratingElement = SeleniumHelper.FindElementSafe(card, _config.RatingXPath);
                var reviewsElement = SeleniumHelper.FindElementSafe(card, _config.ReviewsXPath);

                if (nameElement == null || priceElement == null || urlElement == null || imageElement == null)
                {
                    Console.WriteLine($"[WARNING] {_config.MarketUrl}: Пропущен товар из-за отсутствующих данных.");
                    return null;
                }

                string rawPrice = priceElement.Text.Trim();

                string numericPrice = Regex.Replace(rawPrice, @"[^\d]", "");
                if (!decimal.TryParse(numericPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
                {
                    Console.WriteLine($"[ERROR] {_config.MarketUrl}: Ошибка конвертации цены \"{numericPrice}\".");
                    return null;
                }

                return new ProductDto
                {
                    Marketplace = _config.MarketName,
                    Name = nameElement.Text.Trim(),
                    Brand = brandElement?.Text.Trim() ?? "Неизвестно",
                    Price = price,
                    Url = urlElement.GetAttribute("href"),
                    ImageUrl = imageElement.GetAttribute("src"),
                    Rating = ratingElement?.Text.Trim().Replace(",", ".") ?? "Нет рейтинга",
                    ReviewsCount = reviewsElement?.Text.Trim() ?? "0 отзывов"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка парсинга товара на {_config.MarketUrl}: {ex.Message}");
                return null;
            }
        }
    }
}
