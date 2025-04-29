using PoolScraper.Model;
using PoolScraper.Model.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public interface ISnapshotHourConsolidationPersistency
    {
        Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(DateOnly dateOnly);
        Task<bool> InsertManyAsync(int hour, IEnumerable<ISnapshotWorkerStatus> hourlyUptime);
    }
}