
using CommonUtils.Utils;
using PoolScraper.Domain;

namespace PoolScraper.Service
{
    public class UptimeServiceClient(ILogger<UptimeServiceClient> logger, IUptimeService uptimeService) : IUptimeServiceClient
    {
        public async Task<IEnumerable<IWorkerUptime>> GetDailyUptimeAsync(DateOnly date)
        {
            logger.LogInformation("GetDailyUptimeAsync for date: {date}", date);
            var beginOfToday = date.GetBeginOfDay();
            var endOfToday = date.GetEndOfDay();

            return await uptimeService.GetWorkerUptimeStatsAsync(beginOfToday, endOfToday);
        }

        public async Task<IEnumerable<IUptimePeriod>> GetWorkerUptimeHistoryAsync(long workerId, DateTime from, DateTime to)
        {
            return await uptimeService.GetWorkerUptimeHistoryAsync("powerpool", workerId, from, to);
        }
    }
}
