using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.Service
{
    public interface IPowerPoolScrapingService
    {
        Task FetchAndStoreUserData();
        Task<IEnumerable<PowerPoolUser>> GetDataRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotWorkerStatusAsync(long workerId, DateTime from, DateTime to);
        Task<PowerPoolUser> GetLatestUserDataAsync();
        Task<double> GetTodayCoverageAsync();
        Task RecreateWorkersAsync(PowerPoolUser powerPoolScraping);
    }
}