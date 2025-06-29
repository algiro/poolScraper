using CommonUtils.Utils;

namespace PoolScraper.Domain
{
    public class UptimePeriods
    {
        public static IEnumerable<IUptimePeriod> CreatePeriods(IEnumerable<IWorkerUptimeHistory> workerUptimeHistory)
        {
            if (workerUptimeHistory == null || workerUptimeHistory.IsEmpty())
                return Enumerable.Empty<IUptimePeriod>();

            var orderedHistory = workerUptimeHistory.OrderBy(w => w.Timestamp);
            var periodStart = orderedHistory.First().Timestamp;
            var periodState = orderedHistory.First().IsActive;
            IWorkerUptimeHistory preiousUptime = orderedHistory.First();

            List<IUptimePeriod> periods = new List<IUptimePeriod>();
            foreach (var workerUptime in orderedHistory)
            {
                if (workerUptime.IsActive != periodState)
                {
                    var uptimePeriod = UptimePeriod.Create(periodStart, preiousUptime.Timestamp, periodState);
                    periods.Add(uptimePeriod);
                    periodState = workerUptime.IsActive;
                    periodStart = workerUptime.Timestamp;
                }
                preiousUptime = workerUptime;
            }
            // Add the last period
            var lastUptimePeriod = UptimePeriod.Create(periodStart, orderedHistory.Last().Timestamp, periodState);
            periods.Add(lastUptimePeriod);
            return periods;
        }
    }
}
