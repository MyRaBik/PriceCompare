using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRequestRepository
    {
        Task<Request?> GetByQueryAsync(string query);
        Task AddAsync(Request searchRequest);
        Task SaveChangesAsync();
    }
}
