using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public interface ISnapshotConsolidateServiceClient
    {
        Task ConsolidateHours(DateOnly date);
        Task<IEnumerable<ISnapshotDataConsolidationInfo>> GetSnapshotDataConsolidationInfoAsync(IDateRange dateRange);
        Task<(bool success,string? message)> ConsolidateDays(IDateRange dateRange);
        Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange);
        Task<IEnumerable<ISnapshotWorkerStatus>> GetDailySnapshotAsync(IDateRange dateRange);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);
    }
}