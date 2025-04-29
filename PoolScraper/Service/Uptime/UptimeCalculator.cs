using PoolScraper.Model;
using PoolScraper.Model.Consolidation;

namespace PoolScraper.Service.Uptime
{
    public class UptimeCalculator
    {
        public IEnumerable<IUptimePercentage> CalculateTotUptime(IEnumerable<ISnapshotWorkerStatus> snapshots)
        {
            var filledSnapshots = snapshots.FillTheGaps();
            var dateFrom = filledSnapshots.Min(f => f.DateRange.From);
            var dateTo = filledSnapshots.Max(f => f.DateRange.To);
            var dateRange = DateRange.Create(dateFrom, dateTo);
            return filledSnapshots.GroupBy(s => s.WorkerId)
                .Select(g =>
                {
                    //var dateFrom = g.Min(f => f.DateRange.From);
                    //var dateTo = g.Max(f => f.DateRange.From);
                    //var dateRange = DateRange.Create(dateFrom, dateTo);
                    int total = g.Sum(s => s.Weight());
                    int up = g.Where(s => s.BasicInfo.Hashrate > 0).Sum(s => s.Weight());
                    double percentage = (total == 0) ? 0.0 : (double)up / total * 100.0;
                    return UptimePercentage.Create(g.Key, dateRange,percentage);
                });
        }

    }
}
