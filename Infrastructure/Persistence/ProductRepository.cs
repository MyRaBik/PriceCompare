using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetBySearchRequestIdAsync(int requestId)
        {
            return await _context.Products
                .Include(p => p.PriceHistories)
                .Where(p => p.RequestId == requestId)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<Product> GetByUrlAsync(string url)
        {
            return await _context.Products
                .Include(p => p.PriceHistories)
                .FirstOrDefaultAsync(p => p.Url == url);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public async Task<Product> GetByKeyAsync(string marketplace, string name, string url)
        {
            return await _context.Products
                .Include(p => p.PriceHistories)
                .FirstOrDefaultAsync(p =>
                    p.Marketplace == marketplace &&
                    p.Name == name &&
                    p.Url == url);
        }


        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
