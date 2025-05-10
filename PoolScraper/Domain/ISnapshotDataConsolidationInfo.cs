using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Domain
{
    public interface ISnapshotDataConsolidationInfo
    {
        Granularity Granularity { get; }
        IDateRange DateRange { get; }
    }

    public static class SnapshotDataConsolidationInfo
    {
        public static ISnapshotDataConsolidationInfo Create(this Granularity granularity, IDateRange dateRange)
        {
            return new DefaultSnapshotDataConsolidationInfo(granularity, dateRange);
        }

        private class DefaultSnapshotDataConsolidationInfo(Granularity granularity, IDateRange dateRange) : ISnapshotDataConsolidationInfo
        {
            public Granularity Granularity { get; } = granularity;
            public IDateRange DateRange { get; } = dateRange;
        }
    }
}
