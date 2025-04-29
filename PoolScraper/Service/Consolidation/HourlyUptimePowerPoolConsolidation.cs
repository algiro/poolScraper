using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Uptime;

namespace PoolScraper.Service.Consolidation
{
    public class HourlyUptimePowerPoolConsolidation
    {
        public IEnumerable<(int hour, IEnumerable<IUptimePercentage> percentage)> GetHourlyUptime(IEnumerable<PowerPoolUser> powerPoolScrapings)
        {
            var scrapingHours = powerPoolScrapings.Select(scraping => scraping.FetchedAt.Hour).Distinct().ToList();
            UptimeCalculator uptimeCalculator = new UptimeCalculator();
            foreach (var scrapingHour in scrapingHours)
            {
                var scrapingHourData = powerPoolScrapings.Where(scraping => scraping.FetchedAt.Hour == scrapingHour).ToList();
                var snapshotWorkerStatus = scrapingHourData.AsSnapshotWorkerStatus();
                var workerUptimeResult = uptimeCalculator.CalculateTotUptime(snapshotWorkerStatus);
                yield return (scrapingHour, workerUptimeResult);
            }
        }
    }
}
