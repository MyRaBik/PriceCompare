using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Subscription subscription)
        {
            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Subscription>> GetAllByUserAsync(int userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Request)
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByIdAsync(int id)
        {
            return await _context.Subscriptions.FindAsync(id);
        }

        public async Task<Subscription?> GetByUserAndRequestAsync(int userId, int requestId)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.RequestId == requestId);
        }

        public async Task DeleteAsync(Subscription subscription)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Subscription>> GetAllWithUsersAndRequestsAsync()
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Request)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByUserAndRequestListAsync(int userId, List<int> requestIds)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && requestIds.Contains(s.RequestId));
        }

        //для обновления подписок
        public async Task<List<int>> GetAllRequestIdsWithSubscribersAsync()
        {
            return await _context.Subscriptions
                .Select(s => s.RequestId)
                .Distinct()
                .ToListAsync();
        }

        public async Task ReplaceRequestIdAsync(int oldRequestId, int newRequestId)
        {
            var subs = await _context.Subscriptions
                .Where(s => s.RequestId == oldRequestId)
                .ToListAsync();

            foreach (var s in subs)
            {
                s.RequestId = newRequestId;
            }

            await _context.SaveChangesAsync();
        }
    }
}
