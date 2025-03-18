using Domain;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductParser
    {
        Task<List<Product>> GetCheapestPricesAsync(string productName);
    }
}