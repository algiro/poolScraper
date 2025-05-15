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
                    [FromKeyedServices("hourSnapConsolidation")] ISnapshotConsolidationPersistency snapshotHourConsolidationPersistency,
                    [FromKeyedServices("daySnapConsolidation")] ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency,
                    ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency,
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
                DateTime currentDateTime = date.GetBeginOfDay();
                foreach (var hourlySnapshot in hourlySnapshotConsolidationResult)
                {
                    var snapshotsCount = hourlySnapshot.snapshots.Count();
                    logger.LogInformation("hourlySnapshot  hours#: {hourCount} storing snapshots#: {snapCount}", hourlySnapshot.hour, snapshotsCount);
                    if (snapshotsCount > 0)
                    {
                        await snapshotHourConsolidationPersistency.InsertManyAsync(hourlySnapshot.snapshots);
                        var currentDateRange = DateRange.Create(currentDateTime.AddHours(hourlySnapshot.hour), currentDateTime.AddHours(hourlySnapshot.hour + 1));
                        await snapshotDataConsolidationPersistency.InsertAsync(SnapshotDataConsolidationInfo.Create(Granularity.Hours, currentDateRange));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("ConsolidateHours error: {message} {stackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task ConsolidateDays(IDateRange dateRange)
        {
            try
            {
                var currentDate = DateOnly.FromDateTime(dateRange.From);
                var finalDate = DateOnly.FromDateTime(dateRange.To);

                while (currentDate <= finalDate)
                {
                    logger.LogInformation("ConsolidateDays  processing: {currentDate}", currentDate);
                    var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(currentDate.GetBeginOfDay(), currentDate.GetEndOfDay());
                    var snapshotWorkerStatus = powerPoolScrapings.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap());
                    logger.LogInformation("ConsolidateDays  processing: {currentDate} found #: {snapCount}", currentDate, snapshotWorkerStatus.Count());

                    var dailySnapshotConsolidation = new DailySnapshotsConsolidation();
                    var dailySnapshotConsolidationResult = dailySnapshotConsolidation.GetDailySnapshots(snapshotWorkerStatus);
                    logger.LogInformation("ConsolidateDays  processing: {currentDate} Consolidated #: {consolidCount}", currentDate, dailySnapshotConsolidationResult.Count());
                    if (dailySnapshotConsolidationResult.Count() > 1)
                    {
                        throw new InvalidOperationException("ConsolidateDays  processing: " + currentDate + " Consolidated #:" + dailySnapshotConsolidationResult.Count() + " is greater than 1");
                    }
                    if (dailySnapshotConsolidationResult.Count() == 1)
                    {
                        var daySnapshot = dailySnapshotConsolidationResult.Single();
                        var snapshotsCount = daySnapshot.snapshots.Count();
                        logger.LogInformation("ConsolidateDays daySnapshot  day: {snapDay} snapCount#: {snapCount}", daySnapshot.date, snapshotsCount);
                        if (snapshotsCount > 0)
                        {
                            await snapshotDayConsolidationPersistency.InsertManyAsync(daySnapshot.snapshots);
                            await snapshotDataConsolidationPersistency.InsertAsync(SnapshotDataConsolidationInfo.Create(Granularity.Days, currentDate.AsDateRange()));
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("ConsolidateDays error: {message} {stackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange)
            => await snapshotHourConsolidationPersistency.GetSnapshotAsync(dateRange);
    }
}