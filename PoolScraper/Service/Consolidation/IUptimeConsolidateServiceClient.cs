using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public interface IUptimeConsolidateServiceClient
    {
        Task Consolidate(DateOnly date);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetHourlyUptimeAsync(DateOnly dateOnly);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetDailyWorkerUptimeAsync(DateOnly dateOnly);
    }
}