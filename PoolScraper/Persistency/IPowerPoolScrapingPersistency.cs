using PoolScraper.Model;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.Persistency
{
    public interface IPowerPoolScrapingPersistency
    {
        Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotWorkerStatusAsync(IWorkerId workerId, DateTime from, DateTime to);
        Task<IEnumerable<PowerPoolUser>> GetDataRangeAsync(DateTime from, DateTime to);
        Task<PowerPoolUser> GetLatestUserDataAsync();
        Task<bool> InsertAsync(PowerPoolUser powerPoolUser);
    }
}