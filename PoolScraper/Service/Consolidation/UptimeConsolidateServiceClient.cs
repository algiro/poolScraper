
using CommonUtils.Utils;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service.Uptime;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Domain;

namespace PoolScraper.Service.Consolidation
{
    public class UptimeConsolidateServiceClient(IPowerPoolScrapingService powerPoolScrapingService, IUptimeHourConsolidationPersistency uptimeHourConsolidationPersistency,IWorkerIdMap workerIdMap) : IUptimeConsolidateServiceClient
    {
        public async Task Consolidate(DateOnly date)
        {
            var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
            var hourlyUptimePowerPoolConsolidation = new HourlyUptimePowerPoolConsolidation();
            var hourlyUptimeConsolidation = hourlyUptimePowerPoolConsolidation.GetHourlyUptime(powerPoolScrapings, workerIdMap);

            foreach (var hourlyUptime in hourlyUptimeConsolidation)
            {
                Console.WriteLine("hourlyUptime: " + hourlyUptime.hour);
                await uptimeHourConsolidationPersistency.InsertManyAsync(hourlyUptime.hour,hourlyUptime.percentage);
            }

        }
        public async Task<IEnumerable<IUptimePercentage>> GetHourlyUptimeAsync(DateOnly dateOnly) => await uptimeHourConsolidationPersistency.GetHourlyUptimeAsync(dateOnly);
        
    }
}
