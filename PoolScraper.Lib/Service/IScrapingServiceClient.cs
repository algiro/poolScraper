
using CommonUtils.Utils;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;

namespace PoolScraper.Service
{
    public interface IScrapingServiceClient
    {
        Task FetchAndStoreUserDataAsync();
        Task<PowerPoolScraperInfo> GetLatestUserDataAsync();
        Task<double> GetTodayCoverageAsync();
        Task<(IEnumerable<PowerPoolUser> data, IEnumerable<TimeGap> gap)> GetDayDetailsAsync(DateOnly date);
    }
}