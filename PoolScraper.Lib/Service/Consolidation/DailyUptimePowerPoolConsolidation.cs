using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Uptime;

namespace PoolScraper.Service.Consolidation
{
    public class DailyUptimePowerPoolConsolidation
    {
        public IEnumerable<(DateOnly date, IEnumerable<IUptimePercentage> percentage)> GetDailyUptime(IEnumerable<PowerPoolUser> powerPoolScrapings, IWorkerIdMap workerIdMap)
        {
            var scrapingDates = powerPoolScrapings.Select(scraping => scraping.FetchedAt.Date).Distinct().ToList();
            UptimeCalculator uptimeCalculator = new UptimeCalculator();
            foreach (var scrapingDate in scrapingDates)
            {
                var scrapingDateData = powerPoolScrapings.Where(scraping => scraping.FetchedAt.Date == scrapingDate).ToList();
                var snapshotWorkerStatus = scrapingDateData.AsSnapshotWorkerStatus(workerIdMap);
                var workerUptimeResult = uptimeCalculator.CalculateTotUptime(snapshotWorkerStatus);
                yield return (scrapingDate.ToDateOnly(), workerUptimeResult);
            }
        }

        public IEnumerable<IUptimePercentage> GetDailyUptime(DateOnly date, IEnumerable<ISnapshotWorkerStatus> snapshotWorkerStatus, IWorkerIdMap workerIdMap)
        {
            UptimeCalculator uptimeCalculator = new UptimeCalculator();
            var workerUptimeResult = uptimeCalculator.CalculateTotUptime(snapshotWorkerStatus);
            return workerUptimeResult;
        }
    }
}
