using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Consolidation;
using PoolScraper.Service.Store;

namespace PoolScraper.Service
{
    
    public class WorkersReportService(ILogger<WorkersReportService> logger,
                    [FromKeyedServices("hourSnapConsolidation")] ISnapshotConsolidationPersistency snapshotHourConsolidationPersistency,
                    [FromKeyedServices("daySnapConsolidation")]  ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency,
                    ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency,
                    IWorkerStore workerStore) : IWorkersReportService
    {
        public Task<IEnumerable<ISnapshotDetailedView>> GetLiveSnapshotDetailedViewsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ISnapshotDetailedView>> GetSnapshotDetailedViewsAsync(IDateRange dateRange)
        {
            try
            {
                logger.LogInformation("GetSnapshotDetailedViewsAsync called with date range: {dateRange}", dateRange);                
                var yesterdayEndOfDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)).GetEndOfDay();

                var todayNow = DateTime.Now;
                var correctedFrom = dateRange.From < yesterdayEndOfDay ? dateRange.From : yesterdayEndOfDay;
                var correctedTo = dateRange.To > yesterdayEndOfDay ? yesterdayEndOfDay : dateRange.To;
                var limitedDateRange = DateRange.Create(correctedFrom.GetBeginOfDay(), correctedTo.GetEndOfDay());

                var isTodayIncludedInDateRange = dateRange.From <= todayNow && dateRange.To >= todayNow;
                logger.LogInformation("GetSnapshotDetailedViewsAsync called with date range: {dateRange} limitedDateRange: {dateRangeExToday}", dateRange, limitedDateRange);
                var snapshotDataConsolidationInfos = await snapshotDataConsolidationPersistency.GetSnapshotDataConsolidationInfoAsync(limitedDateRange);
                if (snapshotDataConsolidationInfos.IsEmpty())
                {
                    return [];
                }
                var consolidatedDateRange = DateRange.Create(snapshotDataConsolidationInfos.Min(s => s.DateRange.From), snapshotDataConsolidationInfos.Max(s => s.DateRange.To));

                if (!limitedDateRange.IsSameDateRange(consolidatedDateRange))
                {
                    logger.LogWarning("GetSnapshotDetailedViewsAsync called with date range: {dateRange} limitedDateRange: {limitedDateRange} but consolidated date range: {consolidatedDateRange}", dateRange, limitedDateRange, consolidatedDateRange);
                }

                var dailySnapshot = await snapshotDayConsolidationPersistency.GetSnapshotAsync(limitedDateRange);
                logger.LogInformation("GetSnapshotDetailedViewsAsync {dateRange} daily snapshots#", dailySnapshot.Count());
                IEnumerable<ISnapshotWorkerStatus> todayHourSnapshot = [];
                if (isTodayIncludedInDateRange)
                {
                    var todayRange = DateRange.Create(DateOnly.FromDateTime(todayNow).GetBeginOfDay(), todayNow);
                    todayHourSnapshot = await snapshotHourConsolidationPersistency.GetSnapshotAsync(todayRange);
                    logger.LogInformation("GetSnapshotDetailedViewsAsync {dateRange} today hour snapshots#", todayHourSnapshot.Count());
                }

                var snapshotDetailedViews = dailySnapshot.Concat(todayHourSnapshot).SelectNotNull(s => s.AsSnapshotDetailedView(workerStore));
                return snapshotDetailedViews;
            }
            catch (Exception ex)
            {
                logger.LogError("GetSnapshotDetailedViewsAsync error: {message} {stacktrace}", ex.Message, ex.StackTrace);
                return [];
            }
        }
    }
}
