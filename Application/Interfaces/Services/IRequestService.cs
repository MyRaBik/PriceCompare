using Domain.DTOs;

namespace Application.Interfaces.Services
{
    public interface IRequestService
    {
        Task<List<RequestDto>> GetAllAsync();
        Task<RequestDto?> GetByIdAsync(int id);
        Task<int> AddAsync(RequestDto dto);
        Task DeleteAsync(int id);
        Task<List<ProductDto>> GetProductsByRequestIdAsync(int requestId);
    }
}
