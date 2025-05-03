
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service
{
    public interface IScrapingServiceClient
    {
        Task FetchAndStoreUserDataAsync();
        Task<PowerPoolScraperInfo> GetLatestUserDataAsync();
        Task<double> GetTodayCoverageAsync();
        Task<(IEnumerable<PowerPoolUser> data, IEnumerable<TimeGap> gap)> GetDayDetailsAsync(DateOnly date);
        Task<IEnumerable<WorkerDTO>> GetWorkersAsync();
        Task<bool> ToggleEnableWorkerAsync(IWorkerId workerId);
    }
}