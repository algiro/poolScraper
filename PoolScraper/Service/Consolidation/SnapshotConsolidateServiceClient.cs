using CommonUtils.Utils;
using MongoDB.Driver.Linq;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.View;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PoolScraper.Service.Consolidation
{
    public class SnapshotConsolidateServiceClient(
                    ILogger<SnapshotConsolidateServiceClient> logger,
                    IPowerPoolScrapingService powerPoolScrapingService, 
                    [FromKeyedServices("hourSnapConsolidation")] ISnapshotConsolidationPersistency snapshotHourConsolidationPersistency,
                    [FromKeyedServices("daySnapConsolidation")] ISnapshotConsolidationPersistency snapshotDayConsolidationPersistency,
                    ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency) : ISnapshotConsolidateServiceClient
    {
        public async Task ConsolidateHours(DateOnly date)
        {
            var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
            var snapshotWorkerStatus = powerPoolScrapings.AsSnapshotWorkerStatus();
            var hourlySnapshotConsolidation = new HourlySnapshotsConsolidation();
            var hourlySnapshotConsolidationResult = hourlySnapshotConsolidation.GetHourlySnapshots(snapshotWorkerStatus);
            DateTime currentDateTime = date.GetBeginOfDay();
            foreach (var hourlySnapshot in hourlySnapshotConsolidationResult)
            {
                var snapshotsCount = hourlySnapshot.snapshots.Count();
                logger.LogInformation("hourlySnapshot  hours#: {hourCount} storing snapshots#: {snapCount}",  hourlySnapshot.hour , snapshotsCount);
                if (snapshotsCount > 0)
                {
                    await snapshotHourConsolidationPersistency.InsertManyAsync(hourlySnapshot.snapshots);
                    var currentDateRange = DateRange.Create(currentDateTime.AddHours(hourlySnapshot.hour), currentDateTime.AddHours(hourlySnapshot.hour + 1));
                    await snapshotDataConsolidationPersistency.InsertAsync(SnapshotDataConsolidationInfo.Create(Granularity.Hours, currentDateRange));
                }
            }
        }
        public async Task ConsolidateDays(IDateRange dateRange)
        {
            var currentDate = DateOnly.FromDateTime(dateRange.From);
            var finalDate = DateOnly.FromDateTime(dateRange.To);

            while (currentDate <= finalDate)
            {
                logger.LogInformation("ConsolidateDays  processing: {currentDate}", currentDate);
                var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(currentDate.GetBeginOfDay(),currentDate.GetEndOfDay());
                var snapshotWorkerStatus = powerPoolScrapings.AsSnapshotWorkerStatus();
                logger.LogInformation("ConsolidateDays  processing: {currentDate} found #: {snapCount}", currentDate , snapshotWorkerStatus.Count());

                var dailySnapshotConsolidation = new DailySnapshotsConsolidation();
                var dailySnapshotConsolidationResult = dailySnapshotConsolidation.GetDailySnapshots(snapshotWorkerStatus);
                logger.LogInformation("ConsolidateDays  processing: {currentDate} Consolidated #: {consolidCount}",  currentDate , dailySnapshotConsolidationResult.Count());
                if  (dailySnapshotConsolidationResult.Count() > 1)
                {
                    throw new InvalidOperationException("ConsolidateDays  processing: " + currentDate + " Consolidated #:" + dailySnapshotConsolidationResult.Count() + " is greater than 1");
                }
                if (dailySnapshotConsolidationResult.Count() == 1)
                {
                    var daySnapshot = dailySnapshotConsolidationResult.Single();
                    var snapshotsCount = daySnapshot.snapshots.Count();
                    logger.LogInformation("ConsolidateDays daySnapshot  day: {snapDay} snapCount#: {snapCount}" , daySnapshot.date ,snapshotsCount);
                    if (snapshotsCount > 0)
                    {
                        await snapshotDayConsolidationPersistency.InsertManyAsync(daySnapshot.snapshots);
                        await snapshotDataConsolidationPersistency.InsertAsync(SnapshotDataConsolidationInfo.Create(Granularity.Days, currentDate.AsDateRange()));
                    }
                }
                currentDate = currentDate.AddDays(1);
            }
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange)
            => await snapshotHourConsolidationPersistency.GetSnapshotAsync(dateRange);
    }
}