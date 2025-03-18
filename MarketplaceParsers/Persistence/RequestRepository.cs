using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _context;

        public RequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Request?> GetByQueryAsync(string query)
        {
            return await _context.Request
                .FirstOrDefaultAsync(sr => sr.Query.ToLower() == query.ToLower());
        }

        public async Task AddAsync(Request searchRequest)
        {
            await _context.Request.AddAsync(searchRequest);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
