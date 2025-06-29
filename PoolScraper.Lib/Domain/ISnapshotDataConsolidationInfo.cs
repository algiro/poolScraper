using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Domain
{
    public interface ISnapshotDataConsolidationInfo
    {
        Granularity Granularity { get; }
        IDateRange DateRange { get; }
        int SnapshotCount { get; }
    }

    public static class SnapshotDataConsolidationInfo
    {
        public static ISnapshotDataConsolidationInfo Create(this Granularity granularity, IDateRange dateRange, int snapshotsCount)
        {
            return new DefaultSnapshotDataConsolidationInfo(granularity, dateRange, snapshotsCount);
        }

        private class DefaultSnapshotDataConsolidationInfo(Granularity granularity, IDateRange dateRange, int snapshotsCount) : ISnapshotDataConsolidationInfo
        {
            public Granularity Granularity { get; } = granularity;
            public IDateRange DateRange { get; } = dateRange;
            public int SnapshotCount { get; } = snapshotsCount;
        }
    }
}
