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

        private static IUptimePercentageDetailedView? CreateAverageSnapshot<T>(IGrouping<T, IUptimePercentageDetailedView> group)
        {
            if (group.Count() == 0)
                return null;

            var workerId = group.Key;
            var averageUptime = group.Average(s => s.UptimePercentage);
            var refUptime = group.First();
            return UptimePercentageDetailedView.Create(UptimePercentage.Create(refUptime.WorkerId,refUptime.DateRange, averageUptime), refUptime.Worker);
        }
         
        
    }
}
