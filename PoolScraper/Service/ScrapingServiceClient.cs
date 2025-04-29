using CommonUtils.Utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service
{
    public class ScrapingServiceClient(ILogger<ScrapingServiceClient> logger,IPowerPoolScrapingService powerPoolScrapingService,IWorkersService workerService) : IScrapingServiceClient
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
            DateTime beginOfToday = date.ToDateTime(new TimeOnly(0, 0, 0));
            DateTime endOfToday = date.ToDateTime(new TimeOnly(23, 59, 59));

            var todayData =  await powerPoolScrapingService.GetDataRangeAsync(beginOfToday, endOfToday);
            var gaps = FindTimeGaps(todayData.Select(u => u.FetchedAt), beginOfToday, endOfToday,TimeSpan.FromSeconds(90));
            return (todayData, gaps);
        }


        public IEnumerable<TimeGap> FindTimeGaps(IEnumerable<DateTime> fetchedTimes, DateTime fromDate, DateTime toDate,TimeSpan timeSpanThreshold)
        {
            if (fromDate > toDate)
            {
                throw new ArgumentException("fromDate must be earlier than toDate");
            }
            DateTime actualTime = fromDate;
            List<TimeGap> gaps = new List<TimeGap>();
            foreach (DateTime fetchedTime in fetchedTimes)
            {
                var diff = fetchedTime - actualTime;
                if (diff >= timeSpanThreshold)
                {
                    var gap = new TimeGap() { GapTime = actualTime, MissingSpan = diff };
                    gaps.Add(gap);
                }
                actualTime = fetchedTime;
            }
            return gaps;
        }
        public async Task<IEnumerable<WorkerDTO>> GetWorkersAsync()
        {
            try
            {
                var workers = await workerService.GetWorkersAsync();
                logger.LogInformation("GetWorkers # {count}", workers.Count());
                return workers;
            }
            catch (Exception ex)
            {
                logger.LogError("Error fetching workers data : {message}", ex.Message);
            }
            return Enumerable.Empty<WorkerDTO>();
        }
        public async Task<bool> ToggleEnableWorkerAsync(string poolId,long workerId)
        {
            try
            {
                var done = await workerService.ToggleEnableWorkerAsync(poolId,workerId);
                logger.LogInformation("ToggleEnableWorkerAsync status {done}", done);
                return done;
            }
            catch (Exception ex)
            {
                logger.LogError("Error ToggleEnableWorkerAsync on {workerId}: {message}", workerId, ex.Message);
                return false;
            }
        }
    }
}
