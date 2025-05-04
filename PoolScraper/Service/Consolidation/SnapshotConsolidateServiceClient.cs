using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public class SnapshotConsolidateServiceClient(IPowerPoolScrapingService powerPoolScrapingService, ISnapshotHourConsolidationPersistency snapshotHourConsolidationPersistency) : ISnapshotConsolidateServiceClient
    {
        public async Task Consolidate(DateOnly date)
        {
            var powerPoolScrapings = await powerPoolScrapingService.GetDataRangeAsync(date.GetBeginOfDay(), date.GetEndOfDay());
            var snapshotWorkerStatus = powerPoolScrapings.AsSnapshotWorkerStatus();
            var hourlySnapshotConsolidation = new HourlySnapshotsConsolidation();
            var hourlySnapshotConsolidationResult = hourlySnapshotConsolidation.GetHourlySnapshots(snapshotWorkerStatus);
            foreach (var hourlySnapshot in hourlySnapshotConsolidationResult)
            {
                Console.WriteLine("hourlySnapshot  hours#: " + hourlySnapshot.hour + " storing snapshots#:" + hourlySnapshot.snapshots.Count());
                await snapshotHourConsolidationPersistency.InsertManyAsync(hourlySnapshot.hour, hourlySnapshot.snapshots);
            }
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange)
            => await snapshotHourConsolidationPersistency.GetHourlySnapshotAsync(dateRange);
    }
}