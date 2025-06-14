using CommonUtils.Utils.Logs;
using log4net;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Store;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public interface IUptimeConsolidateServiceClient
    {
        Task ConsolidateHours(DateOnly date);
        Task<IEnumerable<IUptimeDataConsolidationInfo>> GetUptimeDataConsolidationInfoAsync(IDateRange dateRange);
        Task ConsolidateDay(DateOnly date);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetHourlyUptimeAsync(DateOnly dateOnly);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetDailyWorkerUptimeAsync(IDateRange dateRange);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);

    }

    public static class UptimeConsolidateServiceClientHelper
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger<IUptimeConsolidateServiceClient>();
        public static async Task ConsolidateDays(this IUptimeConsolidateServiceClient uptimeConsolidateServiceClient, IDateRange dateRange)
        {
            logger.LogInformation("ConsolidateDays for dateRange: {dateRange}", dateRange);
            var dataConsolidated = await uptimeConsolidateServiceClient.GetUptimeDataConsolidationInfoAsync(dateRange);
            var dayConsolidated = dataConsolidated.Where(d => d.Granularity == Granularity.Days).ToList();
            logger.LogInformation("ConsolidateDays found {count} days already consolidated", dayConsolidated.Count);
            var currentDate = DateOnly.FromDateTime(dateRange.From);
            while (currentDate < DateOnly.FromDateTime(dateRange.To))
            {
                var currentDateRange = currentDate.AsDateRange();
                var hasBeenAlreadyConsolidated = dayConsolidated.Any(d => d.DateRange.Equals(currentDateRange));
                logger.LogInformation("ConsolidateDays date: {date}, already consolidated: {hasBeenAlreadyConsolidated}", currentDate, hasBeenAlreadyConsolidated);
                if (!hasBeenAlreadyConsolidated)
                {
                    await uptimeConsolidateServiceClient.ConsolidateDays(currentDateRange);
                    logger.LogInformation("ConsolidateDays done dateRange: {currentDateRange}", currentDateRange);
                }
                currentDate = currentDate.AddDays(1);
            }
        }

        public static async Task<(bool success, string? message)> ConsolidateDay(DateOnly date, IEnumerable<ISnapshotWorkerStatus> snapshotWorkerStatus, IWorkerStore workerStore, IUptimeDailyConsolidationPersistency uptimeDailyConsolidationPersistency)
        {
            try
            {
                var dailyUptimePowerPoolConsolidation = new DailyUptimePowerPoolConsolidation();
                var dailyUptimeConsolidation = dailyUptimePowerPoolConsolidation.GetDailyUptime(date, snapshotWorkerStatus, workerStore.GetWorkerIdMap());

                logger.LogInformation($"ConsolidateDays dailyUptime: {date}");
                await uptimeDailyConsolidationPersistency.InsertManyAsync(date, dailyUptimeConsolidation);
                return (true, null);
            }
            catch (Exception ex)
            {
                logger.LogError("ConsolidateDays {currentDate} error: {message} {stackTrace}", date, ex.Message, ex.StackTrace);
                return (false, ex.Message);
            }

        }
    }
}