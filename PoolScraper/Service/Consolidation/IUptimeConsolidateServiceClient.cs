using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public interface IUptimeConsolidateServiceClient
    {
        Task ConsolidateHours(DateOnly date);
        Task<IEnumerable<IUptimeDataConsolidationInfo>> GetUptimeDataConsolidationInfoAsync(IDateRange dateRange);
        Task ConsolidateDays(IDateRange dateRange);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetHourlyUptimeAsync(DateOnly dateOnly);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetDailyWorkerUptimeAsync(DateOnly dateOnly);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);

    }
}