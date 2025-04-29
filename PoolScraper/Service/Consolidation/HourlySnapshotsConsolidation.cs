using BlazorBootstrap;
using CommonUtils.Utils;
using PoolScraper.Model;
using PoolScraper.Model.Consolidation;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Uptime;

namespace PoolScraper.Service.Consolidation
{
    public class HourlySnapshotsConsolidation
    {
        public IEnumerable<(int hour, IEnumerable<ISnapshotWorkerStatus> snapshots)> GetHourlySnapshots(IEnumerable<ISnapshotWorkerStatus> sourceSnapshots)
        {
            ValidateInput(sourceSnapshots);

            var filledSnapshots = sourceSnapshots.FillTheGaps();

            var dateFrom = filledSnapshots.Min(f => f.DateRange.From);
            var dateTo = filledSnapshots.Max(f => f.DateRange.To);
            var dateRange = DateRange.Create(dateFrom, dateTo);

            // Group snapshots by hour and WorkerId
            var groupedByHourAndWorker = filledSnapshots
                .GroupBy(s => (Hour: s.DateRange.MiddleDateTime().Hour, s.WorkerId))
                .Select(group =>
                {
                    // Calculate the average hashrate for this group
                    double weightedHashRateTotal = group.Sum(x => x.Weight() * x.BasicInfo.Hashrate);
                    double totalWeight = group.Sum(x => x.Weight());

                    if (totalWeight > 61)
                    {
                        throw new InvalidOperationException("Total weight cannot be greater than 60 (minutes)");
                    }

                    double averageHashrate = weightedHashRateTotal / totalWeight;

                    // Pick a representative item from the group for other fields
                    var representative = group.First();
                    var baseDate = representative.DateRange.From.Date;
                    var groupDateRange = DateRange.Create(
                        baseDate.AddHours(group.Key.Hour),
                        baseDate.AddHours(group.Key.Hour + 1)
                    );

                    // Create the aggregated snapshot with calculated average
                    return (
                        Hour: group.Key.Hour,
                        AggregatedSnapshot: SnapshotWorkerStatus.Create(
                            representative.WorkerId,
                            Granularity.Hours,
                            groupDateRange,
                            WorkerBasicInfo.Create(averageHashrate, 0)
                        ) as ISnapshotWorkerStatus
                    );
                })
                .ToList();

            // Group the aggregated snapshots by hour
            var hourlyGroups = groupedByHourAndWorker
                .GroupBy(g => g.Hour)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(item => item.AggregatedSnapshot).ToList() as IEnumerable<ISnapshotWorkerStatus>
                );

            // Ensure all 24 hours are represented in the result
            var result = new List<(int hour, IEnumerable<ISnapshotWorkerStatus> snapshots)>();

            for (int hour = 0; hour < 24; hour++)
            {
                if (hourlyGroups.ContainsKey(hour))
                {
                    result.Add((hour, hourlyGroups[hour]));
                }
                else
                {
                    result.Add((hour, Enumerable.Empty<ISnapshotWorkerStatus>()));
                }
            }

            return result;
        }

        private static void ValidateInput(IEnumerable<ISnapshotWorkerStatus> sourceSnapshots)
        {
            var areSnapshotsInTheSameDay = sourceSnapshots.All(snapshot => snapshot.DateRange.From.Date == sourceSnapshots.First().DateRange.From.Date);
            if (!areSnapshotsInTheSameDay)
            {
                throw new ArgumentException("All snapshots must be in the same day.");
            }
            if (sourceSnapshots.HasOverlapGreaterThan(TimeSpan.FromMinutes(1)))
            {
                throw new InvalidOperationException("Snapshots have overlapping time ranges greater than one minute.");
            }
        }
    }
}
