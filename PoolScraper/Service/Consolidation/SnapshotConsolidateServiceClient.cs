using CommonUtils.Utils;
using MongoDB.Driver.Linq;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Store;
using PoolScraper.View;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PoolScraper.Service.Consolidation
{
    public class SnapshotConsolidateServiceClient(
                    ILogger<SnapshotConsolidateServiceClient> logger,
                    IPowerPoolScrapingService powerPoolScrapingService,
                    ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency,
                    [FromKeyedServices("hourSnapConsolidation")] ISnapshotConsolidationPersistency snapshotHourConsolidationPersistency,
                    [FromKeyedServices("daySnapConsolidation")] ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency,                    
                    IWorkerStore workerStore) : ISnapshotConsolidateServiceClient
    {
        public async Task ConsolidateHours(DateOnly date)
        {
            try
            {
                var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
                var snapshotWorkerStatus = powerPoolScrapings.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap());
                var hourlySnapshotConsolidation = new HourlySnapshotsConsolidation();
                var hourlySnapshotConsolidationResult = hourlySnapshotConsolidation.GetHourlySnapshots(snapshotWorkerStatus);
                foreach (var hourlySnapshot in hourlySnapshotConsolidationResult)
                {
                    var snapshotsCount = hourlySnapshot.snapshots.Count();
                    logger.LogInformation("hourlySnapshot  hours#: {hourCount} storing snapshots#: {snapCount}", hourlySnapshot.hour, snapshotsCount);
                    if (snapshotsCount > 0)
                    {
                        await snapshotHourConsolidationPersistency.InsertManyAsync(hourlySnapshot.snapshots,sourceCount:-1);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("ConsolidateHours error: {message} {stackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<(bool success, string? message)> ConsolidateDay(DateOnly date)
        {
            bool success = true;
            try
            {
                logger.LogInformation("ConsolidateDays  processing: {currentDate}", date);
                var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
                var snapshotWorkerStatus = powerPoolScrapings.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap());
                await SnapshotConsolidateServiceClientHelper.ConsolidateDay(date, minutesCount:powerPoolScrapings.Count(), snapshotWorkerStatus, snapshotDayConsolidationPersistency);
            }
            catch (Exception ex)
            {
                success = false;
                logger.LogError("ConsolidateDays {currentDate} error: {message} {stackTrace}", date, ex.Message, ex.StackTrace);
                throw;
            }
             
            return (success, null);
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange)
            => await snapshotHourConsolidationPersistency.GetSnapshotAsync(dateRange);

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetDailySnapshotAsync(IDateRange dateRange)
            => await snapshotDayConsolidationPersistency.GetSnapshotAsync(dateRange);

        public async Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange)
            => await snapshotDayConsolidationPersistency.RemoveDayConsolidationAsync(dateRange);

        public async Task<IEnumerable<ISnapshotDataConsolidationInfo>> GetSnapshotDataConsolidationInfoAsync(IDateRange dateRange)
            => await snapshotDataConsolidationPersistency.GetSnapshotDataConsolidationInfoAsync(dateRange);
    }
}