using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Service
{
    public class WorkersReport
    {
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerWorker(IEnumerable<ISnapshotDetailedView> snapshots) {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => s.WorkerId)
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<IWorkerId>(group);
                });
            return groupedByHourAndWorker;
        }

        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerModel(IEnumerable<ISnapshotDetailedView> snapshots)
        {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => s.Worker.Model)
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<IWorkerModel>(group);
                });
            return groupedByHourAndWorker;
        }
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerModelAndDate(IEnumerable<ISnapshotDetailedView> snapshots)
        {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => new { s.Worker.Model, s.DateRange })
                .SelectNotNull(group =>
                {
                    double weightedHashRateTotal = group.Sum(x => x.Weight() * x.BasicInfo.Hashrate);
                    double totalWeight = group.Sum(x => x.Weight());
                    var representative = group.First();
                    var workerGrouped = Worker.Create("","",0,group.Key.Model.ToString(),0, group.Key.Model,Farm.UNKNOWN);
                    var baseDateFrom = group.Min(g => g.DateRange.From);
                    var baseDateTo = group.Max(g => g.DateRange.To);
                    var groupDateRange = DateRange.Create(baseDateFrom, baseDateTo);
                    var snapshotAverage = SnapshotWorkerStatus.Create(
                        WorkerId.UNINITIALIZED,
                        Granularity.Custom,
                        groupDateRange,
                        WorkerBasicInfo.Create(weightedHashRateTotal / totalWeight, 0)
                    );
                    return snapshotAverage.AsSnapshotDetailedView(workerGrouped);
                });
            return groupedByHourAndWorker;
        }

        private static ISnapshotDetailedView? CreateAverageSnapshot<T>(IGrouping<T, ISnapshotDetailedView> group)
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
        }
    }
}
