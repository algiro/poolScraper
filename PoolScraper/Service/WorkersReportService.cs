using CommonUtils.Utils;
using PoolScraper.Model;
using PoolScraper.Persistency;
using PoolScraper.Service.Consolidation;
using PoolScraper.Service.Store;

namespace PoolScraper.Service
{
    public class WorkersReportService(ILogger<WorkersReportService> logger, ISnapshotConsolidateServiceClient snapshotConsolidateServiceClient, IWorkerStore workerStore) : IWorkersReportService
    {
        public Task<IEnumerable<ISnapshotDetailedView>> GetLiveSnapshotDetailedViewsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ISnapshotDetailedView>> GetSnapshotDetailedViewsAsync(IDateRange dateRange)
        {
            logger.LogInformation("GetSnapshotDetailedViewsAsync called with date range: {dateRange}", dateRange);
            var snapshots = await snapshotConsolidateServiceClient.GetHourlySnapshotAsync(dateRange);
            logger.LogInformation("GetSnapshotDetailedViewsAsync {dateRange} snapshots#", snapshots.Count());

            var snapshotDetailedViews = snapshots.SelectNotNull(s => s.AsSnapshotDetailedView(workerStore));
            return snapshotDetailedViews;
        }
    }
}
