using CommonUtils.Utils.Logs;
using PoolScraper.Domain;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Consolidation;
using PoolScraper.Service.Store;

namespace PoolScraper.Service
{
    public class AdminService(
        IPowerPoolScrapingService powerPoolScrapingService, 
        [FromKeyedServices("daySnapConsolidation")] ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency, 
        IUptimeDailyConsolidationPersistency uptimeDailyConsolidationPersistency,
        IWorkerStore workerStore) : IAdminService
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger<IAdminService>();
        public async Task<bool> RestoreCollectionsFromScraping(IDateRange dateRange)
        {
            try
            {
                logger.LogInformation("RestoreCollectionsFromScraping for dateRange: {dateRange}", dateRange);
                var dates = dateRange.GetDatesWithinRange().ToList();
                foreach (var date in dates)
                {
                    logger.LogInformation("RestoreCollectionsFromScraping processing date: {date}", date);
                    var dateRangeForDay = date.AsDateRange();
                    var dayScrapings = (await powerPoolScrapingService.GetDataRangeAsync(dateRangeForDay.From, dateRangeForDay.To)).ToList();
                    if (dayScrapings.Count < 1410) // 1410 is the minimum number of scrapings expected for a full day (24 hours * 60 minutes / 1 minutes per scraping)
                    {
                        logger.LogWarning("!!!!! RestoreCollectionsFromScraping for date: {date} found scrapings count: {count} is less than expected minimum 1410", date, dayScrapings.Count);                        
                    }
                    logger.LogInformation("RestoreCollectionsFromScraping processing date: {date} Completed ", date);

                    var lastScrapingOfTheDay = dayScrapings.OrderBy(s => s.FetchedAt).LastOrDefault();
                    await powerPoolScrapingService.RecreateWorkersAsync(lastScrapingOfTheDay!);

                    var snapshotWorkerStatus = dayScrapings.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap());
                    await SnapshotConsolidateServiceClientHelper.ConsolidateDay(date, snapshotWorkerStatus, snapshotDayConsolidationPersistency);
                    await UptimeConsolidateServiceClientHelper.ConsolidateDay(date, snapshotWorkerStatus, workerStore, uptimeDailyConsolidationPersistency);
                    logger.LogInformation("RestoreCollectionsFromScraping processing date: {date} Completed ", date);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("RestoreCollectionsFromScraping error: {message} {stackTrace}", ex.Message, ex.StackTrace);
                return false;
            }
        }
    }
}
