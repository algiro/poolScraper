

using CommonUtils.Utils;

namespace PoolScraper.Service
{
    public class ScheduledService(IScrapingServiceClient scrapintServiceClient) : BackgroundService
    {
        private CancellationToken cancellationToken;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            this.cancellationToken = stoppingToken;
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
