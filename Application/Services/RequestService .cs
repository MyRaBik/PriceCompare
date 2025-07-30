using Domain.DTOs;
using Domain.Entities;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly IPriceHistoryRepository _priceHistoryRepository;

        public RequestService(
            IRequestRepository repository,
            IProductRepository productRepository,
            IPriceHistoryRepository priceHistoryRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _priceHistoryRepository = priceHistoryRepository;
        }

        public async Task<List<RequestDto>> GetAllAsync()
        {
            var requests = await _repository.GetAllAsync();
            return requests.Select(r => new RequestDto
            {
                Id = r.Id,
                Query = r.Query,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<RequestDto?> GetByIdAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return null;

            return new RequestDto
            {
                Id = request.Id,
                Query = request.Query,
                CreatedAt = request.CreatedAt
            };
        }

        public async Task<int> AddAsync(RequestDto dto)
        {
            var request = new Request
            {
                Query = dto.Query,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(request);
            await _repository.SaveChangesAsync();

            return request.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request != null)
            {
                await _repository.DeleteAsync(request);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<List<ProductDto>> GetProductsByRequestIdAsync(int requestId)
        {
            var products = await _productRepository.GetBySearchRequestIdAsync(requestId);
            var result = new List<ProductDto>();

            foreach (var product in products)
            {
                var history = await _priceHistoryRepository.GetHistoryByProductAsync(product.Id);

                result.Add(new ProductDto
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
                });
            }

            return result;
        }
    }
}
