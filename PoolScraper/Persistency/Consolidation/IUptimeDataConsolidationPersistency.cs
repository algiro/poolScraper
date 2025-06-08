using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public interface IUptimeDataConsolidationPersistency
    {
        public Task<IEnumerable<IUptimeDataConsolidationInfo>> GetUptimeDataConsolidationInfoAsync(IDateRange dateRange);
        public Task<bool> RemoveDataConsolidationInfoAsync(IDateRange dateRange,Granularity granularity);
        public Task<bool> InsertAsync(IUptimeDataConsolidationInfo snapshotDataConsolidationInfo);
    }
}
