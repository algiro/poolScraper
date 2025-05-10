using PoolScraper.Domain;

namespace PoolScraper.Persistency.Consolidation
{
    public interface ISnapshotDataConsolidationPersistency
    {
        public Task<IEnumerable<ISnapshotDataConsolidationInfo>> GetSnapshotDataConsolidationInfoAsync(IDateRange dateRange);
        public Task<bool> InsertAsync(ISnapshotDataConsolidationInfo snapshotDataConsolidationInfo);
    }
}
