using MongoDB.Bson.IO;
using PoolScraper.Model;

namespace PoolScraper.Service
{
    public interface IWorkersReportService
    {
        public Task<IEnumerable<ISnapshotDetailedView>> GetSnapshotDetailedViewsAsync(IDateRange dateRange);
        public Task<IEnumerable<ISnapshotDetailedView>> GetLiveSnapshotDetailedViewsAsync();

    }
}
