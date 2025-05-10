using BlazorBootstrap;
using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Uptime;

namespace PoolScraper.Service.Consolidation
{
    public class DailySnapshotsConsolidation
    {
        public IEnumerable<(DateOnly date, IEnumerable<ISnapshotWorkerStatus> snapshots)> GetDailySnapshots(IEnumerable<ISnapshotWorkerStatus> sourceSnapshots)
        {
            if (sourceSnapshots == null || sourceSnapshots.IsEmpty())
            {
                return Enumerable.Empty<(DateOnly date, IEnumerable<ISnapshotWorkerStatus> snapshots)>();
            }

            var filledSnapshots = sourceSnapshots.FillTheGaps();

            var dateFrom = filledSnapshots.Min(f => f.DateRange.From);
            var dateTo = filledSnapshots.Max(f => f.DateRange.To);
            var dateRange = DateRange.Create(dateFrom, dateTo);

            // Group snapshots by hour and WorkerId
            var groupedByHourAndWorker = filledSnapshots
                .GroupBy(s => (Date: s.DateRange.MiddleDateTime().Date.ToDateOnly(), s.WorkerId))
                .Select(group =>
                {
                    // Calculate the average hashrate for this group
                    double weightedHashRateTotal = group.Sum(x => x.Weight() * x.BasicInfo.Hashrate);
                    double totalWeight = group.Sum(x => x.Weight());

                    if (totalWeight > 1441)
                    {
                        throw new InvalidOperationException("Total weight cannot be greater than 1441 (minutes) (24h)");
                    }

                    double averageHashrate = weightedHashRateTotal / totalWeight;

                    // Pick a representative item from the group for other fields
                    var representative = group.First();
                    var baseDate = representative.DateRange.From.Date;
                    var groupDateRange = group.Key.Date.AsDateRange();
                    

                    // Create the aggregated snapshot with calculated average
                    return (
                        date: group.Key.Date,
                        AggregatedSnapshot: SnapshotWorkerStatus.Create(
                            representative.WorkerId,
                            Granularity.Days,
                            groupDateRange,
                            WorkerBasicInfo.Create(averageHashrate, 0)
                        ) as ISnapshotWorkerStatus
                    );
                })
                .ToList();

            // Group the aggregated snapshots by hour
            var dailyGroups = groupedByHourAndWorker
                .GroupBy(g => g.date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(item => item.AggregatedSnapshot).ToList() as IEnumerable<ISnapshotWorkerStatus>
                );

            // Ensure all days in the range are represented in the result
            var result = new List<(DateOnly date, IEnumerable<ISnapshotWorkerStatus> snapshots)>();
            var currentDate = dateRange.From.Date.ToDateOnly();
            while (currentDate <=  dateRange.To.Date.ToDateOnly())
            {
                if (dailyGroups.ContainsKey(currentDate))
                {
                    result.Add((currentDate, dailyGroups[currentDate]));
                }
                else
                {
                    result.Add((currentDate, Enumerable.Empty<ISnapshotWorkerStatus>()));
                }
                currentDate = currentDate.AddDays(1);
            }
            return result;
        }
    }
}
