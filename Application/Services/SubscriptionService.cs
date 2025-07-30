using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.DTOs.Subscriptions;
using Domain.Entities;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IRequestRepository _requestRepository;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            IRequestRepository requestRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _requestRepository = requestRepository;
        }

        public async Task<SubscriptionDto> AddAsync(CreateSubscriptionDto dto, int userId)
        {
            var newRequest = await _requestRepository.GetByIdAsync(dto.RequestId)
                ?? throw new ArgumentException("Запрос не найден");

            // находим все Request с таким же Query
            var sameRequests = await _requestRepository.GetByQueryAsync(newRequest.Query);

            // есть ли хотя бы одна подписка на эти Request?
            var existing = await _subscriptionRepository
                .GetByUserAndRequestListAsync(userId, sameRequests.Select(r => r.Id).ToList());

            if (existing != null)
                throw new InvalidOperationException("Вы уже подписаны на такой же запрос.");

            var subscription = new Subscription
            {
                UserId = userId,
                RequestId = dto.RequestId
            };

            await _subscriptionRepository.AddAsync(subscription);

            return new SubscriptionDto
            {
                Id = subscription.Id,
                RequestId = subscription.RequestId,
                Query = newRequest.Query
            };
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAllByUserAsync(int userId)
        {
            var subscriptions = await _subscriptionRepository.GetAllByUserAsync(userId);

            return subscriptions.Select(s => new SubscriptionDto
            {
                Id = s.Id,
                RequestId = s.RequestId,
                Query = s.Request?.Query ?? "неизвестный запрос"
            });
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);

            if (subscription == null || subscription.UserId != userId)
                throw new Exception("Подписка не найдена или доступ запрещён");

            await _subscriptionRepository.DeleteAsync(subscription);
        }

        public async Task<IEnumerable<AdminSubscriptionGroupDto>> GetAllGroupedAsync()
        {
            var all = await _subscriptionRepository.GetAllWithUsersAndRequestsAsync();

            return all
                .GroupBy(s => s.User)
                .Select(g => new AdminSubscriptionGroupDto
                {
                    UserId = g.Key.Id,
                    Email = g.Key.Email,
                    Subscriptions = g.Select(s => new SubscriptionDto
                    {
                        Id = s.Id,
                        RequestId = s.RequestId,
                        Query = s.Request?.Query ?? "неизвестно"
                    }).ToList()
                });
        }
    }
}
