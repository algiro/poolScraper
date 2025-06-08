using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Service
{
    public class SnapshotWorkersReport
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
            var groupByModel = snapshots
                .GroupBy(s => s.Worker.Model)
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<IWorkerModel>(group);
                });
            return groupByModel;
        }
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerLocationAndAlgo(IEnumerable<ISnapshotDetailedView> snapshots)
        {
            var groupByLocation = snapshots
                .GroupBy(s => $"{s.Worker.Farm.Location}.{s.Worker.Algorithm}")
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<string>(group);
                });
            return groupByLocation;
        }
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerModelAndDate(IEnumerable<ISnapshotDetailedView> snapshots)
        {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => $"{s.Worker.Model}.{s.DateRange}")
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<string>(group);
                });
            return groupedByHourAndWorker;
        }
        public IEnumerable<ISnapshotDetailedView> CalculateAveragePerLocationAndDate(IEnumerable<ISnapshotDetailedView> snapshots)
        {
            var groupedByHourAndWorker = snapshots
                .GroupBy(s => $"{s.Worker.Farm.Location}.{s.Worker.Algorithm}.{s.DateRange}")
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<string>(group);
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
