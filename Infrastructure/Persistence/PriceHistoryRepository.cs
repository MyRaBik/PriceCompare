using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Repositories;


namespace Infrastructure.Persistence
{
    public class PriceHistoryRepository : IPriceHistoryRepository
    {
        private readonly AppDbContext _context;

        public PriceHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PriceHistory history)
        {
            await _context.PriceHistory.AddAsync(history);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task TrimPriceHistoryAsync(int productId, int maxCount)
        {
            var histories = await _context.PriceHistory
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip(maxCount)
                .ToListAsync();

            if (histories.Any())
            {
                _context.PriceHistory.RemoveRange(histories);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<PriceHistory>> GetHistoryByProductAsync(int productId)
        {
            return await _context.PriceHistory
                .Where(h => h.ProductId == productId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

    }
}
