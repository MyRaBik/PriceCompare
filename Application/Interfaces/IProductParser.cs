using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductParser
    {
        Task<List<Product>> GetCheapestPricesAsync(string productName);
    }
}