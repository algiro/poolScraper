
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
        IWorkerStore workerStore) : IUptimeConsolidateServiceClient
    {
        public async Task Consolidate(DateOnly date)
        {
            var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
            logger.LogInformation($"Consolidate: {date} found scrapings: {powerPoolScrapings.Count()}");

            var hourlyUptimePowerPoolConsolidation = new HourlyUptimePowerPoolConsolidation();
            var hourlyUptimeConsolidation = hourlyUptimePowerPoolConsolidation.GetHourlyUptime(powerPoolScrapings, workerStore.GetWorkerIdMap());

            foreach (var hourlyUptime in hourlyUptimeConsolidation)
            {
                logger.LogInformation($"hourlyUptime: {date}.{hourlyUptime.hour}");
                await uptimeHourConsolidationPersistency.InsertManyAsync(hourlyUptime.hour,hourlyUptime.percentage);
            }

        }
        public async Task<IEnumerable<IUptimePercentageDetailedView>> GetHourlyUptimeAsync(DateOnly dateOnly)
        {
            var uptimesPercentage = await uptimeHourConsolidationPersistency.GetHourlyUptimeAsync(dateOnly);
            return uptimesPercentage.SelectNotNull(u => u.AsUptimePercentageDetailedView(workerStore));
        }
        public async Task<IEnumerable<IUptimePercentageDetailedView>> GetDailyWorkerUptimeAsync(DateOnly dateOnly)
        {
            var hourlyUptimesPercentage = await uptimeHourConsolidationPersistency.GetHourlyUptimeAsync(dateOnly);
            var dateOnlyAsRange = dateOnly.AsDateRange();
            var dailyAverageUpTimePerWorker = hourlyUptimesPercentage
                .GroupBy(u => u.WorkerId)
                .Select(g => UptimePercentage.Create(g.Key, dateOnlyAsRange, g.Average(u => u.UptimePercentage)));

            return dailyAverageUpTimePerWorker.SelectNotNull(u => u.AsUptimePercentageDetailedView(workerStore));
        }

    }
}
