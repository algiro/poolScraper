using CommonUtils.Utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service
{
    public class ScrapingServiceClient(ILogger<ScrapingServiceClient> logger,IPowerPoolScrapingService powerPoolScrapingService) : IScrapingServiceClient
    {

        public async Task FetchAndStoreUserDataAsync()
        {            
            logger.LogInformation("Fetching and storing user data...");
            // Fetch and store data
            await powerPoolScrapingService.FetchAndStoreUserData();
        }

        public async Task<PowerPoolScraperInfo> GetLatestUserDataAsync()
        {
            
            // Retrieve latest data
            var userData = await powerPoolScrapingService.GetLatestUserDataAsync();
            logger.LogInformation($"Data fetched at: {userData.FetchedAt}");
            PowerPoolScraperInfo scraperInfo = new PowerPoolScraperInfo(userData);
            return scraperInfo;

        }

        public async Task<double> GetTodayCoverageAsync() 
        {
            return await powerPoolScrapingService.GetTodayCoverageAsync();
        }
        public async Task<(IEnumerable<PowerPoolUser> data, IEnumerable<TimeGap> gap)> GetDayDetailsAsync(DateOnly date)
        {
            DateTime beginOfToday = date.GetBeginOfDay();
            DateTime endOfToday = date.GetEndOfDay();

            var todayData =  await powerPoolScrapingService.GetDataRangeAsync(beginOfToday, endOfToday);
            var gaps = DateUtils.FindTimeGaps(todayData.Select(u => u.FetchedAt), beginOfToday, endOfToday,TimeSpan.FromSeconds(90));
            return (todayData, gaps);
        }
    }
}
