using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Domain
{
    public interface IUptimeDataConsolidationInfo
    {
        Granularity Granularity { get; }
        IDateRange DateRange { get; }
        int Count { get; }
    }

    public static class UptimeDataConsolidationInfo
    {
        public static IUptimeDataConsolidationInfo Create(this Granularity granularity, IDateRange dateRange, int count)
        {
            return new DefaultUptimeDataConsolidationInfo(granularity, dateRange, count);
        }

        private class DefaultUptimeDataConsolidationInfo(Granularity granularity, IDateRange dateRange, int snapshotsCount) : IUptimeDataConsolidationInfo
        {
            public Granularity Granularity { get; } = granularity;
            public IDateRange DateRange { get; } = dateRange;
            public int Count { get; } = snapshotsCount;
        }
    }
}
