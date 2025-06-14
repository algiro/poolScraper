using CommonUtils.Utils;
using CommonUtils.Utils.Logs;
using PoolScraper.Domain;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Persistency.Utils;
using PoolScraper.Service.Consolidation;
using PoolScraper.Service.Store;

namespace PoolScraper.Service
{
    public class AdminService(
        IMongoUtils mongoUtils,
        IPowerPoolScrapingService powerPoolScrapingService, 
        [FromKeyedServices("daySnapConsolidation")] ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency, 
        IUptimeDailyConsolidationPersistency uptimeDailyConsolidationPersistency,
        IAppEventsPersistency appEventsPersistency,
        IWorkerStore workerStore) : IAdminService
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger<IAdminService>();
        public async Task<bool> RestoreCollectionsFromScraping(IDateRange dateRange)
        {
            try
            {
                await appEventsPersistency.InsertAsync(AppEvent.Create(AppEventType.Warning, $"RestoreCollectionsFromScraping for dateRange: {dateRange}"));

                logger.LogInformation("RestoreCollectionsFromScraping for dateRange: {dateRange}", dateRange);
                var dates = dateRange.GetDatesWithinRange().ToList();
                foreach (var date in dates)
                {
                    logger.LogInformation("RestoreCollectionsFromScraping processing date: {date}", date);
                    var dateRangeForDay = date.AsDateRange();
                    var dayScrapings = (await powerPoolScrapingService.GetDataRangeAsync(dateRangeForDay.From, dateRangeForDay.To)).ToList();
                    if (dayScrapings.Count < 1410) // 1410 is the minimum number of scrapings expected for a full day (24 hours * 60 minutes / 1 minutes per scraping)
                    {
                        var gaps = DateUtils.FindTimeGaps(dayScrapings.Select(u => u.FetchedAt), dateRangeForDay.From, dateRangeForDay.To, TimeSpan.FromSeconds(90));
                        var warnMsg = $"!!!!! RestoreCollectionsFromScraping for date: {date} found scrapings count: {dayScrapings.Count} is less than expected minimum 1410 gaps: {string.Join(';', gaps)}";
                        logger.LogWarning(warnMsg);
                        await appEventsPersistency.InsertAsync(AppEvent.Create(AppEventType.Warning, warnMsg));
                    }
                    var lastScrapingOfTheDay = dayScrapings.OrderBy(s => s.FetchedAt).LastOrDefault();
                    await powerPoolScrapingService.RecreateWorkersAsync(lastScrapingOfTheDay!);

                    var snapshotWorkerStatus = dayScrapings.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap());
                    await SnapshotConsolidateServiceClientHelper.ConsolidateDay(date, minutesCount: dayScrapings.Count, snapshotWorkerStatus, snapshotDayConsolidationPersistency);
                    await UptimeConsolidateServiceClientHelper.ConsolidateDay(date, snapshotWorkerStatus, workerStore, uptimeDailyConsolidationPersistency);
                    var completedMsg = $"RestoreCollectionsFromScraping for date: {date} completed!";
                    logger.LogInformation(completedMsg);
                    await appEventsPersistency.InsertAsync(AppEvent.Create(AppEventType.Info, completedMsg));
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
