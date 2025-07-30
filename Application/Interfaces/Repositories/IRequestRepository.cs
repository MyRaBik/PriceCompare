using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IRequestRepository
    {
        Task<List<Request>> GetAllAsync();
        Task<Request?> GetByIdAsync(int id);
        Task AddAsync(Request request);
        Task DeleteAsync(Request request);
        Task SaveChangesAsync();
        Task<Request?> GetLastByQueryAsync(string query);
        Task<List<Request>> GetByQueryAsync(string query);
    }
}
