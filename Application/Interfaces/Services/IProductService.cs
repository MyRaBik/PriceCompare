using Domain.DTOs;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductSearchResultDto> SearchProductAsync(string query, bool force = false);
        Task<ProductDto?> GetProductWithHistoryAsync(string url);
        Task<List<ProductDto>> GetProductsByRequestIdAsync(int requestId);
        Task<List<PriceHistoryDto>> GetPriceHistoryAsync(int productId);
    }
}
