using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public interface ISnapshotDataConsolidationPersistency
    {
        public Task<IEnumerable<ISnapshotDataConsolidationInfo>> GetSnapshotDataConsolidationInfoAsync(IDateRange dateRange);
        public Task<bool> RemoveDataConsolidationInfoAsync(IDateRange dateRange,Granularity granularity);
        public Task<bool> InsertAsync(ISnapshotDataConsolidationInfo snapshotDataConsolidationInfo);
    }
}
