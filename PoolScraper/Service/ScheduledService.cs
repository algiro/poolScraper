

using CommonUtils.Utils;
using PoolScraper.Config;

namespace PoolScraper.Service
{
    public class ScheduledService(IInitApp initApp, IScrapingServiceClient scrapintServiceClient) : BackgroundService
    {
        private CancellationToken cancellationToken;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            this.cancellationToken = stoppingToken;
            initApp.Init();
            await ScheduleScrapingAsync();
        }

        private async Task ScheduleScrapingAsync()
        {
            await TaskUtils.RepeatRun(ImportAction, every: TimeSpan.FromMinutes(1), startAfter: TimeSpan.Zero, cancellationToken: cancellationToken);

        }
        private async Task ImportAction()
        {
            await scrapintServiceClient.FetchAndStoreUserDataAsync();
        }
    }
}
