using System.Globalization;
using MarketplaceParsers;
using Domain.DTOs;
using OpenQA.Selenium;

namespace Infrastructure.Parsers
{
    public class WildberriesSeleniumParser : BaseSeleniumParser
    {
        public WildberriesSeleniumParser(WebDriverPool driverPool)
            : base(driverPool, "configs/wildberries_config.json") { }

        protected override void PerformSorting(IWebDriver driver)
        {
            // Wildberries НАДО ДОПИЛИТЬ (как яндекс)
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

                string rawPrice = priceElement.Text
                    .Replace("₽", "").Replace("\u00A0", "").Replace("\u2009", "")
                    .Replace("\u202F", "").Replace(" ", "").Replace(",", ".").Trim();

                if (!decimal.TryParse(rawPrice, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out decimal price))
                {
                    Console.WriteLine($"[ERROR] {_config.MarketUrl}: Ошибка конвертации цены: {rawPrice}");
                    return null;
                }

                return new ProductDto
                {
                    Marketplace = _config.MarketName,
                    Name = nameElement.Text.Trim().TrimStart('/').Trim(),
                    Brand = brandElement?.Text.Trim() ?? "Неизвестно",
                    Price = price,
                    Url = urlElement.GetAttribute("href"),
                    ImageUrl = imageElement.GetAttribute("src"),
                    Rating = ratingElement?.Text.Trim() ?? "Нет рейтинга",
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
