using PoolScraper.Domain;

namespace PoolScraper.Persistency.Consolidation
{
    public interface ISnapshotHourConsolidationPersistency
    {
        Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange);
        Task<bool> InsertManyAsync(int hour, IEnumerable<ISnapshotWorkerStatus> hourlyUptime);
    }
}