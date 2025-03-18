using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetBySearchRequestIdAsync(int requestId);
        Task AddAsync(Product product);
        Task SaveChangesAsync();
    }
}
