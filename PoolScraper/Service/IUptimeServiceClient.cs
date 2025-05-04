using PoolScraper.Domain;

namespace PoolScraper.Service
{
    public interface IUptimeServiceClient
    {
        Task<IEnumerable<IWorkerUptime>> GetDailyUptimeAsync(DateOnly date);
        Task<IEnumerable<IUptimePeriod>> GetWorkerUptimeHistoryAsync(long workerId, DateTime from, DateTime to);
    }
}
