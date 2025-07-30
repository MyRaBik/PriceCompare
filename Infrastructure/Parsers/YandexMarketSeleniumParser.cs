using System.Globalization;
using Domain.DTOs;
using System.Text.RegularExpressions;
using MarketplaceParsers;
using OpenQA.Selenium;

namespace Infrastructure.Parsers
{
    public class YandexMarketSeleniumParser : BaseSeleniumParser
    {
        public YandexMarketSeleniumParser(WebDriverPool driverPool)
            : base(driverPool, "configs/yandex_config.json") { }

        protected override void PerformSorting(IWebDriver driver) //ЯНДЕКС САМ СПРАВЛЯЕТСЯ ПРИ ПРАВИЛЬНОМ НАЗВАНИИ
        {
            try
            {
                //var sortButton = SeleniumHelper.WaitForClickableElement(By.XPath(_config.SortButtonXPath), driver);
                //sortButton.Click();

                var cheaperOption = SeleniumHelper.WaitForClickableElement(By.XPath(_config.SortCheapestXPath), driver);
                cheaperOption.Click();

                //driver.FindElement(By.TagName("body"))?.Click(); // Закрываем выпадающий список

                SeleniumHelper.WaitForElements(By.XPath(_config.ProductElementsXPath), driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка сортировки на {_config.MarketUrl}: {ex.Message}");
            }
        }

        protected override ProductDto ParseProductCard(IWebElement card)
        {
            try
            {
                string name = SeleniumHelper.FindElementSafe(card, _config.NameXPath)?.Text.Trim();
                if (string.IsNullOrEmpty(name)) return null;

                var priceElement = SeleniumHelper.FindElementSafe(card, _config.PriceXPath);
                if (priceElement == null) return null;

                decimal price = TryParsePrice(priceElement.Text);
                if (price == 0) return null;

                string url = GetProductUrl(card);
                if (string.IsNullOrEmpty(url)) return null;

                string imageUrl = SeleniumHelper.FindElementSafe(card, _config.ImageXPath)?.GetAttribute("src") ?? "Нет изображения";

                var reviewsContainer = SeleniumHelper.FindElementSafe(card, _config.ReviewsContainer);
                string ratingText = reviewsContainer?.FindElement(By.XPath(_config.RatingXPath))?.Text.Trim() ?? "Нет рейтинга";
                string reviewsText = ExtractReviewsCount(reviewsContainer);

                return new ProductDto
                {
                    Marketplace = _config.MarketName,
                    Name = name,
                    Brand = "Неизвестно",
                    Price = price,
                    Url = url,
                    ImageUrl = imageUrl,
                    Rating = ratingText,
                    ReviewsCount = reviewsText
                };
            }
            catch
            {
                return null;
            }
        }

        private string GetProductUrl(IWebElement card)
        {
            var urlElement = SeleniumHelper.FindElementSafe(card, _config.UrlXPath);
            string url = urlElement?.GetAttribute("href") ?? card.FindElements(By.XPath(".//a")).FirstOrDefault()?.GetAttribute("href");

            return string.IsNullOrEmpty(url) ? "" : url.StartsWith("http") ? url : _config.MarketUrl + url;
        }

        private string ExtractReviewsCount(IWebElement reviewsContainer)
        {
            if (reviewsContainer == null) return "0 отзывов";
            string rawReviews = reviewsContainer.FindElement(By.XPath(_config.ReviewsXPath))?.Text.Trim() ?? "0";
            return Regex.Match(rawReviews, @"\d+").Value ?? "0 отзывов";
        }

        private decimal TryParsePrice(string priceText)
        {
            string numericPrice = Regex.Replace(priceText, @"[^\d]", "");
            return decimal.TryParse(numericPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) ? price : 0;
        }
    }
}
