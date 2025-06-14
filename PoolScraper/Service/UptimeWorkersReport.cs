using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Service
{
    public class UptimeWorkersReport
    {
        public IEnumerable<IUptimePercentageDetailedView> CalculateAveragePerWorker(IEnumerable<IUptimePercentageDetailedView> uptimes) {
            var groupedByHourAndWorker = uptimes
                .GroupBy(s => s.WorkerId)
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<IWorkerId>(group);
                });
            return groupedByHourAndWorker;
        }
        public IEnumerable<IUptimePercentageDetailedView> CalculateAveragePerModel(IEnumerable<IUptimePercentageDetailedView> uptimes)
        {
            var groupByModel = uptimes
                .GroupBy(s => s.Worker.Model)
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<IWorkerModel>(group);
                });
            return groupByModel;
        }

        public IEnumerable<IUptimePercentageDetailedView> CalculateAveragePerModelAndDate(IEnumerable<IUptimePercentageDetailedView> uptimes)
        {
            var groupedByHourAndWorker = uptimes
                .GroupBy(s => $"{s.Worker.Model}.{s.DateRange}")
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<string>(group);
                });
            return groupedByHourAndWorker;
        }
        public IEnumerable<IUptimePercentageDetailedView> CalculateAveragePerLocationAndAlgo(IEnumerable<IUptimePercentageDetailedView> uptimes)
        {
            var groupByLocation = uptimes
                .GroupBy(s => $"{s.Worker.Farm.Location}.{s.Worker.Algorithm}")
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<string>(group);
                });
            return groupByLocation;
        }
        public IEnumerable<IUptimePercentageDetailedView> CalculateAveragePerLocationAndDate(IEnumerable<IUptimePercentageDetailedView> uptimes)
        {
            var groupedByHourAndWorker = uptimes
                .GroupBy(s => $"{s.Worker.Farm.Location}.{s.Worker.Algorithm}.{s.DateRange}")
                .SelectNotNull(group =>
                {
                    return CreateAverageSnapshot<string>(group);
                });
            return groupedByHourAndWorker;
        }
        private static IUptimePercentageDetailedView? CreateAverageSnapshot<T>(IGrouping<T, IUptimePercentageDetailedView> group)
        {
            if (group.Count() == 0)
                return null;

            var workerId = group.Key;
            var averageUptime = group.Average(s => s.UptimePercentage);
            var representative = group.First();
            var baseDateFrom = group.Min(g => g.DateRange.From);
            var baseDateTo = group.Max(g => g.DateRange.To);
            var groupDateRange = DateRange.Create(baseDateFrom, baseDateTo);

            return UptimePercentageDetailedView.Create(UptimePercentage.Create(representative.WorkerId, groupDateRange, averageUptime), representative.Worker);
        }
         
        
    }
}
