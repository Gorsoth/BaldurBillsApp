namespace BaldurBillsApp.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class NbpRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _rateService;

        public NbpRateBackgroundService(IServiceProvider rateService)
        {
            _rateService = rateService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _rateService.CreateScope())
                {
                    var nbpRateService = scope.ServiceProvider.GetRequiredService<NbpRateService>();
                    await nbpRateService.FetchAndSaveRatesAsync();
                }

                // Wait for the next run (for example, daily)
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
