using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public interface ISnapshotConsolidationPersistency
    {
        Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotAsync(IDateRange dateRange);
        Task<bool> InsertManyAsync(IEnumerable<ISnapshotWorkerStatus> hourlyUptime);
    }
}