using Domain.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRequestRepository _searchRequestRepository;
        private readonly IProductParser _parser;
        private readonly IPriceHistoryRepository _priceHistoryRepository;

        public ProductService(
            IProductRepository productRepository,
            IRequestRepository searchRequestRepository,
            IProductParser parser,
            IPriceHistoryRepository priceHistoryRepository)
        {
            _productRepository = productRepository;
            _searchRequestRepository = searchRequestRepository;
            _parser = parser;
            _priceHistoryRepository = priceHistoryRepository;
        }

        public async Task<ProductSearchResultDto> SearchProductAsync(string query, bool force = false)
        {
            var recentRequest = await _searchRequestRepository.GetLastByQueryAsync(query);

            if (!force && recentRequest != null && recentRequest.CreatedAt > DateTime.UtcNow.AddHours(-24))
            {
                var productsFromDb = await _productRepository.GetBySearchRequestIdAsync(recentRequest.Id);
                return new ProductSearchResultDto
                {
                    RequestId = recentRequest.Id,
                    Products = await ConvertToDtoAsync(productsFromDb)
                };
            }

            var parsedProducts = await _parser.GetCheapestPricesAsync(query);

            var newRequest = new Request
            {
                Query = query,
                CreatedAt = DateTime.UtcNow
            };

            await _searchRequestRepository.AddAsync(newRequest);
            await _searchRequestRepository.SaveChangesAsync();

            var savedProducts = new List<Product>();

            // 1. Добавляем новые продукты
            foreach (var parsedProduct in parsedProducts)
            {
                parsedProduct.RequestId = newRequest.Id;
                var normalizedUrl = NormalizeUrl(parsedProduct.Url);
                parsedProduct.Url = normalizedUrl;

                var existingProduct = await _productRepository.GetByUrlAsync(normalizedUrl);
                if (existingProduct != null)
                {
                    existingProduct.RequestId = newRequest.Id;
                    existingProduct.Price = parsedProduct.Price;
                    existingProduct.Rating = parsedProduct.Rating;
                    existingProduct.Reviews = parsedProduct.Reviews;

                    _productRepository.Update(existingProduct);
                    savedProducts.Add(existingProduct);
                }
                else
                {
                    await _productRepository.AddAsync(parsedProduct);
                    savedProducts.Add(parsedProduct);
                }
            }

            // 2. Сохраняем продукты, чтобы у новых был ProductId
            await _productRepository.SaveChangesAsync();

            // 3. Добавляем PriceHistory
            foreach (var product in savedProducts)
            {
                var history = new PriceHistory
                {
                    ProductId = product.Id, // уже гарантированно есть в БД
                    Price = product.Price,
                    CreatedAt = DateTime.UtcNow
                };

                await _priceHistoryRepository.AddAsync(history);
            }

            // 4. Сохраняем историю
            await _priceHistoryRepository.SaveChangesAsync();

            return new ProductSearchResultDto
            {
                RequestId = newRequest.Id,
                Products = await ConvertToDtoAsync(savedProducts)
            };
        }

        public async Task<ProductDto?> GetProductWithHistoryAsync(string url)
        {
            var product = await _productRepository.GetByUrlAsync(url);
            if (product == null)
                return null;

            var history = await _priceHistoryRepository.GetHistoryByProductAsync(product.Id);

            return new ProductDto
            {
                Name = product.Name,
                Marketplace = product.Marketplace,
                Brand = product.Brand,
                Price = product.Price,
                Url = product.Url,
                ImageUrl = product.Image,
                Rating = product.Rating.ToString(),
                ReviewsCount = product.Reviews.ToString(),
                PriceHistory = history.Select(h => new PriceHistoryDto
                {
                    Price = h.Price,
                    CreatedAt = h.CreatedAt
                }).ToList()
            };
        }

        public async Task<List<ProductDto>> GetProductsByRequestIdAsync(int requestId)
        {
            var products = await _productRepository.GetBySearchRequestIdAsync(requestId);
            return await ConvertToDtoAsync(products);
        }

        public async Task<List<PriceHistoryDto>> GetPriceHistoryAsync(int productId)
        {
            var history = await _priceHistoryRepository.GetHistoryByProductAsync(productId);
            return history.Select(h => new PriceHistoryDto
            {
                Price = h.Price,
                CreatedAt = h.CreatedAt
            }).ToList();
        }

        private static string NormalizeUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.GetLeftPart(UriPartial.Path); // Убираем query-параметры
            }
            catch
            {
                return url;
            }
        }

        private async Task<List<ProductDto>> ConvertToDtoAsync(List<Product> products)
        {
            var result = new List<ProductDto>();

            foreach (var product in products)
            {
                var history = await _priceHistoryRepository.GetHistoryByProductAsync(product.Id);

                var dto = new ProductDto
                {
                    Name = product.Name,
                    Brand = product.Brand,
                    Marketplace = product.Marketplace,
                    Price = product.Price,
                    Url = product.Url,
                    ImageUrl = product.Image,
                    Rating = product.Rating.ToString(),
                    ReviewsCount = product.Reviews.ToString(),
                    PriceHistory = history.Select(h => new PriceHistoryDto
                    {
                        Price = h.Price,
                        CreatedAt = h.CreatedAt
                    }).ToList()
                };

                result.Add(dto);
            }

            return result;
        }
    }
}
