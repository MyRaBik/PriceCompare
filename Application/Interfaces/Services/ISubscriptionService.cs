using Domain.DTOs.Subscriptions;

namespace Application.Interfaces.Services
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto> AddAsync(CreateSubscriptionDto dto, int userId);
        Task<IEnumerable<SubscriptionDto>> GetAllByUserAsync(int userId);
        Task DeleteAsync(int id, int userId);
        Task<IEnumerable<AdminSubscriptionGroupDto>> GetAllGroupedAsync();
    }
}
