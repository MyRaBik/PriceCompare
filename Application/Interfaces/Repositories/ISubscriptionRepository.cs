using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ISubscriptionRepository
    {
        Task AddAsync(Subscription subscription);
        Task<IEnumerable<Subscription>> GetAllByUserAsync(int userId);
        Task<Subscription?> GetByIdAsync(int id);
        Task<Subscription?> GetByUserAndRequestAsync(int userId, int requestId);
        Task DeleteAsync(Subscription subscription);
        Task<List<Subscription>> GetAllWithUsersAndRequestsAsync();
        Task<Subscription?> GetByUserAndRequestListAsync(int userId, List<int> requestIds);

        //для обновления подписок
        Task<List<int>> GetAllRequestIdsWithSubscribersAsync();
        Task ReplaceRequestIdAsync(int oldRequestId, int newRequestId);
    }
}
