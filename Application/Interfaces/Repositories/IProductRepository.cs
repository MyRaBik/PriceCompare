using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetBySearchRequestIdAsync(int requestId);
        Task AddAsync(Product product);
        Task SaveChangesAsync();
        Task<Product> GetByUrlAsync(string url);
        void Update(Product product);
        Task<Product> GetByKeyAsync(string marketplace, string name, string url);
    }
}
