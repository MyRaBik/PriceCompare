namespace Domain.DTOs
{
    public class ProductDto
    {
        public string Marketplace { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Rating { get; set; }
        public string ReviewsCount { get; set; }
        public List<PriceHistoryDto> PriceHistory { get; set; } = new();
    }
}
