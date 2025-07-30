using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarketplaceParsers
{
    public static class SeleniumHelper
    {
        private const int _defaultTimeout = 15;

        public static ChromeOptions GetChromeOptions()
        {
            var options = new ChromeOptions();

            options.AddArgument("--headless=chrome"); 
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-animations");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--blink-settings=imagesEnabled=false");
            options.AddArgument("--log-level=3");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--single-process");
            options.AddArgument("--no-zygote");

            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/113.0.0.0 Safari/537.36");

            return options;
        }

        public static IWebElement WaitForElement(By by, IWebDriver driver, int timeout = _defaultTimeout)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(ExpectedConditions.ElementExists(by));
        }

        public static IWebElement WaitForClickableElement(By by, IWebDriver driver, int timeout = _defaultTimeout)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(ExpectedConditions.ElementToBeClickable(by));
        }

        public static IReadOnlyCollection<IWebElement> WaitForElements(By by, IWebDriver driver, int timeout = _defaultTimeout)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
        }

        public static IWebElement FindElementSafe(IWebElement parent, string xPath)
        {
            try { return parent.FindElements(By.XPath(xPath)).FirstOrDefault(); }
            catch { return null; }
        }

        public static double GetBrandSimilarity(string candidate, string reference)
        {
            int commonChars = candidate.Intersect(reference).Count();
            return (double)commonChars / Math.Max(candidate.Length, reference.Length);
        }
    }
}
