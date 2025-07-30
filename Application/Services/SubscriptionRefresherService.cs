using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class SubscriptionRefresherService : ISubscriptionRefresherService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly IProductService _productService;

        public SubscriptionRefresherService(
            ISubscriptionRepository subscriptionRepository,
            IRequestRepository requestRepository,
            IProductService productService)
        {
            _subscriptionRepository = subscriptionRepository;
            _requestRepository = requestRepository;
            _productService = productService;
        }

        public async Task RefreshAllAsync()
        {
            var requestIds = await _subscriptionRepository.GetAllRequestIdsWithSubscribersAsync();

            foreach (var oldRequestId in requestIds)
            {
                var oldRequest = await _requestRepository.GetByIdAsync(oldRequestId);
                if (oldRequest == null) continue;

                var result = await _productService.SearchProductAsync(oldRequest.Query, force: true); //форс(тру) - безусловное обновление
                var newRequestId = result.RequestId;

                await _subscriptionRepository.ReplaceRequestIdAsync(oldRequestId, newRequestId);

                // (позже по возможности) удалить старый Request
                // await _requestRepository.DeleteAsync(oldRequestId);
            }

            Console.WriteLine($"[INFO] Подписки обновлены для {requestIds.Count} запросов.");
        }
    }
}
