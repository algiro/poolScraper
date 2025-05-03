using CommonUtils.Utils;
using PoolScraper.Model;
using PoolScraper.Model.Consolidation;

namespace PoolScraper.Service
{
    public class WorkersReport
    {
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerWorker(IEnumerable<ISnapshotDetailedView> snapshots) {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => s.WorkerId)
                .SelectNotNull(group =>
                {
                    // Calculate the average hashrate for this group
                    double weightedHashRateTotal = group.Sum(x => x.Weight() * x.BasicInfo.Hashrate);
                    double totalWeight = group.Sum(x => x.Weight());
                    var representative = group.First();
                    var baseDateFrom = group.Min(g => g.DateRange.From);
                    var baseDateTo = group.Max(g => g.DateRange.To);
                    var groupDateRange = DateRange.Create(baseDateFrom, baseDateTo);
                    var snapshotAverage = SnapshotWorkerStatus.Create(
                        representative.WorkerId,
                        Granularity.Custom,
                        groupDateRange,
                        WorkerBasicInfo.Create(weightedHashRateTotal / totalWeight, 0)
                    ) ;
                    return snapshotAverage.AsSnapshotDetailedView(representative.Worker);
                });
            return groupedByHourAndWorker;
        }
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerModel(IEnumerable<ISnapshotDetailedView> snapshots)
        {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => s.Worker.Model)
                .SelectNotNull(group =>
                {
                    // Calculate the average hashrate for this group
                    double weightedHashRateTotal = group.Sum(x => x.Weight() * x.BasicInfo.Hashrate);
                    double totalWeight = group.Sum(x => x.Weight());
                    var representative = group.First();
                    var baseDateFrom = group.Min(g => g.DateRange.From);
                    var baseDateTo = group.Max(g => g.DateRange.To);
                    var groupDateRange = DateRange.Create(baseDateFrom, baseDateTo);
                    var snapshotAverage = SnapshotWorkerStatus.Create(
                        representative.WorkerId,
                        Granularity.Custom,
                        groupDateRange,
                        WorkerBasicInfo.Create(weightedHashRateTotal / totalWeight, 0)
                    );
                    return snapshotAverage.AsSnapshotDetailedView(representative.Worker);
                });
            return groupedByHourAndWorker;
        }

    }
}
