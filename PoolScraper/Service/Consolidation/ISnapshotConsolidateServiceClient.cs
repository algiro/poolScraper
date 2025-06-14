using CommonUtils.Utils.Logs;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Store;
using PoolScraper.View;
using System.Diagnostics;

namespace PoolScraper.Service.Consolidation
{
    public interface ISnapshotConsolidateServiceClient
    {
        Task ConsolidateHours(DateOnly date);
        Task<IEnumerable<ISnapshotDataConsolidationInfo>> GetSnapshotDataConsolidationInfoAsync(IDateRange dateRange);
        Task<(bool success,string? message)> ConsolidateDay(DateOnly date);
        Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange);
        Task<IEnumerable<ISnapshotWorkerStatus>> GetDailySnapshotAsync(IDateRange dateRange);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);
    }

    public static class SnapshotConsolidateServiceClientHelper
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger<ISnapshotConsolidateServiceClient>();

        public static async Task<(bool success, string? message)> ConsolidateDateRange(this ISnapshotConsolidateServiceClient snapshotConsolidateServiceClient,IDateRange dateRange)
        {
            var dates = dateRange.GetDatesWithinRange().ToList();
            logger.LogInformation("ConsolidateDateRange for dateRange: {dateRange}", dateRange);
            foreach (var date in dates)
            {
                var result = await snapshotConsolidateServiceClient.ConsolidateDay(date);
                if (!result.success)
                {
                    logger.LogError("ConsolidateDateRange failed for date: {date}, message: {message}", date, result.message);
                    return (false, result.message);
                }
            }
            return (true, null);
        }
        /* ConsolidateDay method that takes a date and a collection of PowerPoolUser objects.
        This is useful for consolidating data for a specific day based on the provided user data (that could be fetched once). */
        public static async Task<(bool success, string? message)> ConsolidateDay(DateOnly date, int minutesCount, IEnumerable<ISnapshotWorkerStatus> snapshotWorkerStatus, ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency)
        {
            try
            {
                logger.LogInformation("ConsolidateDays  processing: {currentDate} found #: {snapCount}", date, snapshotWorkerStatus.Count());

                var dailySnapshotConsolidation = new DailySnapshotsConsolidation();
                var dailySnapshotConsolidationResult = dailySnapshotConsolidation.GetDailySnapshots(snapshotWorkerStatus);
                logger.LogInformation("ConsolidateDays  processing: {currentDate} Consolidated #: {consolidCount}", date, dailySnapshotConsolidationResult.Count());
                if (dailySnapshotConsolidationResult.Count() > 1)
                {
                    throw new InvalidOperationException("ConsolidateDays  processing: " + date + " Consolidated #:" + dailySnapshotConsolidationResult.Count() + " is greater than 1");
                }
                if (dailySnapshotConsolidationResult.Count() == 1)
                {
                    var daySnapshot = dailySnapshotConsolidationResult.Single();
                    var snapshotsCount = daySnapshot.snapshots.Count();
                    logger.LogInformation("ConsolidateDays daySnapshot  day: {snapDay} snapCount#: {snapCount}", daySnapshot.date, snapshotsCount);
                    if (snapshotsCount > 0)
                    {
                        var snapInserted = await snapshotDayConsolidationPersistency.InsertManyAsync(daySnapshot.snapshots, minutesCount);
                        if (!snapInserted)
                        {
                            logger.LogError("ConsolidateDays {currentDate} error: failed to insert snapshots or data", date);
                        }
                        else
                        {
                            logger.LogInformation("ConsolidateDays {currentDate} inserted: {snapCount}", date, snapshotsCount);
                        }
                    }
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                var error = $"ConsolidateDays {date} error: {ex.Message} {ex.StackTrace}";
                logger.LogError(error);
                return (false, error);
                throw;
            }

        }
    }
}