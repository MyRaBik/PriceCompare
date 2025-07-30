using Newtonsoft.Json;

namespace MarketplaceParsers
{
    public class ParserConfig
    {
        public string MarketName { get; set; }
        public string MarketUrl { get; set; }
        public string SearchBoxXPath { get; set; }
        public string ProductElementsXPath { get; set; }
        public string NameXPath { get; set; }
        public string PriceXPath { get; set; }
        public string ImageXPath { get; set; }
        public string UrlXPath { get; set; }
        public string BrandXPath { get; set; }
        public string ReviewsContainer { get; set; }
        public string RatingXPath { get; set; }
        public string ReviewsXPath { get; set; }

        // кнопки сортировки (пока только для яндекса)
        public string SortButtonXPath { get; set; }
        public string SortCheapestXPath { get; set; }

        // константы скроллинга
        public int MaxScrollAttempts { get; set; }
        public int ScrollDelayMs { get; set; }

        // сколько товаров брать
        public int NumberOfProducts { get; set; }

        // константы для имитации действия пользователя
        public int WaitingBeforeSearching { get; set; }
        public bool WaitCardsBeforeSearch { get; set; }
        public int WaitingForAction { get; set; }

        // константа для таймаута для каждого драйвера (с момента выделения этого драйвера)
        public int ParserTimeoutMs { get; set; }

        public static ParserConfig LoadConfig(string path)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"Конфигурационный файл не найден: {fullPath}");

                string json = File.ReadAllText(fullPath);
                return JsonConvert.DeserializeObject<ParserConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка загрузки конфигурации {path}: {ex.Message}");
                return null;
            }
        }
    }
}
