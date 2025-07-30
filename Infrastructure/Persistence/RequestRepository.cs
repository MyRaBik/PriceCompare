using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _context;

        public RequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Request>> GetAllAsync()
        {
            return await _context.Request.ToListAsync();
        }

        public async Task<Request?> GetByIdAsync(int id)
        {
            return await _context.Request.FindAsync(id);
        }

        public async Task AddAsync(Request request)
        {
            await _context.Request.AddAsync(request);
        }

        public async Task DeleteAsync(Request request)
        {
            _context.Request.Remove(request);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Request?> GetLastByQueryAsync(string query)
        {
            return await _context.Request
                .Where(r => r.Query == query)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Request>> GetByQueryAsync(string query)
        {
            return await _context.Request
                .Where(r => r.Query.ToLower() == query.ToLower())
                .ToListAsync();
        }
    }
}
