
using CommonUtils.Utils;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;

namespace PoolScraper.Service
{
    public interface IFetchServiceClient
    {
        Task FetchAndStoreUserDataAsync();
    }
    public interface IScrapingServiceClient : IFetchServiceClient
    {
        Task<PowerPoolScraperInfo> GetLatestUserDataAsync();
        Task<double> GetTodayCoverageAsync();
        Task<(IEnumerable<PowerPoolUser> data, IEnumerable<TimeGap> gap)> GetDayDetailsAsync(DateOnly date);
    }
}