
using CommonUtils.Utils;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service.Uptime;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Domain;
using PoolScraper.Service.Store;

namespace PoolScraper.Service.Consolidation
{
    public class UptimeConsolidateServiceClient(
        ILogger<UptimeConsolidateServiceClient> logger,
        IPowerPoolScrapingService powerPoolScrapingService, 
        IUptimeHourConsolidationPersistency uptimeHourConsolidationPersistency,
        IUptimeDailyConsolidationPersistency uptimeDailyConsolidationPersistency,
        IUptimeDataConsolidationPersistency uptimeDataConsolidationPersistency,
        IWorkerStore workerStore) : IUptimeConsolidateServiceClient
    {
        public async Task ConsolidateHours(DateOnly date)
        {
            var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
            logger.LogInformation($"ConsolidateHours: {date} found scrapings: {powerPoolScrapings.Count()}");

            var hourlyUptimePowerPoolConsolidation = new HourlyUptimePowerPoolConsolidation();
            var hourlyUptimeConsolidation = hourlyUptimePowerPoolConsolidation.GetHourlyUptime(powerPoolScrapings, workerStore.GetWorkerIdMap());

            foreach (var hourlyUptime in hourlyUptimeConsolidation)
            {
                logger.LogInformation($"ConsolidateHours hourlyUptime: {date}.{hourlyUptime.hour}");
                await uptimeHourConsolidationPersistency.InsertManyAsync(hourlyUptime.hour,hourlyUptime.percentage);
            }
        }
        public async Task ConsolidateDays(IDateRange dateRange)
        {
            var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(dateRange.From,dateRange.To);
            logger.LogInformation($"ConsolidateDays: {dateRange} found scrapings: {powerPoolScrapings.Count()}");

            var dailyUptimePowerPoolConsolidation = new DailyUptimePowerPoolConsolidation();
            var dailyUptimeConsolidation = dailyUptimePowerPoolConsolidation.GetDailyUptime(powerPoolScrapings, workerStore.GetWorkerIdMap());

            foreach (var dailyUptime in dailyUptimeConsolidation)
            {
                logger.LogInformation($"ConsolidateDays dailyUptime: {dailyUptime.date}");
                await uptimeDailyConsolidationPersistency.InsertManyAsync(dailyUptime.date, dailyUptime.percentage);
            }
        }

        public async Task<IEnumerable<IUptimePercentageDetailedView>> GetHourlyUptimeAsync(DateOnly dateOnly)
        {
            var uptimesPercentage = await uptimeHourConsolidationPersistency.GetHourlyUptimeAsync(dateOnly);
            return uptimesPercentage.SelectNotNull(u => u.AsUptimePercentageDetailedView(workerStore));
        }
        public async Task<IEnumerable<IUptimePercentageDetailedView>> GetDailyWorkerUptimeAsync(DateOnly dateOnly)
        {
            var hourlyUptimesPercentage = await uptimeDailyConsolidationPersistency.GetDailyUptimeAsync(dateOnly);
            return hourlyUptimesPercentage.SelectNotNull(u => u.AsUptimePercentageDetailedView(workerStore));
        }

        public Task<IEnumerable<IUptimeDataConsolidationInfo>> GetUptimeDataConsolidationInfoAsync(IDateRange dateRange)
            =>  uptimeDataConsolidationPersistency.GetUptimeDataConsolidationInfoAsync(dateRange);
        

        public async Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange)
            => await uptimeDailyConsolidationPersistency.RemoveDayConsolidationAsync(dateRange);
    }
}
