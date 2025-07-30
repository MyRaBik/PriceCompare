using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPriceHistoryRepository
    {
        Task AddAsync(PriceHistory history);
        Task SaveChangesAsync();
        Task<List<PriceHistory>> GetHistoryByProductAsync(int productId);
        Task TrimPriceHistoryAsync(int productId, int maxCount);

    }
}
