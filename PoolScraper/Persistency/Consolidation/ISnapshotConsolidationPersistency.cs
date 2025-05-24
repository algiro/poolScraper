using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public interface ISnapshotConsolidationPersistency
    {
        Granularity Granularity { get; }
        Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotAsync(IDateRange dateRange);
        Task<bool> InsertManyAsync(IEnumerable<ISnapshotWorkerStatus> hourlyUptime);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);
    }
}