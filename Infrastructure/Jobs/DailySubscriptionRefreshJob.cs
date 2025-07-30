using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Services;

namespace Infrastructure.Jobs
{
    public class DailySubscriptionRefreshJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DailySubscriptionRefreshJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1); // каждое "завтра" в 00:00
                var delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var refresher = scope.ServiceProvider.GetRequiredService<ISubscriptionRefresherService>();
                await refresher.RefreshAllAsync();
            }


            //Для проверки работы автообновления (спустя 10 секунд - обновление)

            /*
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var refresher = scope.ServiceProvider.GetRequiredService<ISubscriptionRefresherService>();

                Console.WriteLine("[TEST] Запуск фонового обновления подписок (тест)");
                await refresher.RefreshAllAsync();
            }*/
        }
    }
}
