using PoolScraper.Model;

namespace PoolScraper.Service
{
    public interface IUptimeService
    {
        Task<IEnumerable<IWorkerUptime>> GetWorkerUptimeStatsAsync(DateTime from, DateTime to);
        Task<IEnumerable<IUptimePeriod>> GetWorkerUptimeHistoryAsync(string poolId, long workerId, DateTime from, DateTime to);
    }
}
