using CommonUtils.Utils;
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
                Console.WriteLine("hourlySnapshot: " + hourlySnapshot.hour);
                await snapshotHourConsolidationPersistency.InsertManyAsync(hourlySnapshot.hour, hourlySnapshot.snapshots);
            }
        }

        public Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(DateOnly dateOnly)
        {
            throw new NotImplementedException();
        }
    }
}