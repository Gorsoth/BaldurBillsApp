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
            // Delay task until next day at a specific time (e.g., 2 AM)
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    var now = DateTime.UtcNow;
            //    var nextRun = now.AddDays(1).Date.AddHours(2); // Adjust time here if needed
            //    var delay = nextRun - now;

            //    if (delay > TimeSpan.Zero)
            //    {
            //        await Task.Delay(delay, stoppingToken);
            //    }

            //    await _rateService.FetchAndSaveRatesAsync();
            //}
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
