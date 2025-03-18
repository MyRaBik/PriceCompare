using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRequestRepository _searchRequestRepository;
        private readonly IProductParser _parser;

        public ProductService(
            IProductRepository productRepository,
            IRequestRepository searchRequestRepository,
            IProductParser parser)
        {
            _productRepository = productRepository;
            _searchRequestRepository = searchRequestRepository;
            _parser = parser;
        }

        public async Task<List<Product>> SearchProductAsync(string query)
        {
            var existingSearchRequest = await _searchRequestRepository.GetByQueryAsync(query);
            if (existingSearchRequest != null)
            {
                Console.WriteLine($"[INFO] Найден запрос в БД: {query}");
                return await _productRepository.GetBySearchRequestIdAsync(existingSearchRequest.Id);
            }

            Console.WriteLine($"[INFO] Запрос {query} отсутствует в БД. Запускаем парсинг...");
            var parsedProducts = await _parser.GetCheapestPricesAsync(query);

            var searchRequest = new Request
            {
                Query = query,
                CreatedAt = DateTime.UtcNow
            };
            await _searchRequestRepository.AddAsync(searchRequest);
            await _searchRequestRepository.SaveChangesAsync();

            foreach (var parsedProduct in parsedProducts)
            {
                parsedProduct.RequestId = searchRequest.Id;
                await _productRepository.AddAsync(parsedProduct);
            }
            await _productRepository.SaveChangesAsync();

            return parsedProducts;
        }
    }
}
