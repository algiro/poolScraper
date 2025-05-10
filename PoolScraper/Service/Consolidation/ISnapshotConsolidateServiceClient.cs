using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public interface ISnapshotConsolidateServiceClient
    {
        Task ConsolidateHours(DateOnly date);
        Task ConsolidateDays(IDateRange dateRange);
        Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange);
    }
}