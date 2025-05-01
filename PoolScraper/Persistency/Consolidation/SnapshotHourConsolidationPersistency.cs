using MongoDB.Driver;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model;
using PoolScraper.Model.Consolidation;
using log4net;
using PoolScraper.View;
using CommonUtils.Utils;
using PoolScraper.Components.Pages;

namespace PoolScraper.Persistency.Consolidation
{
    public class SnapshotHourConsolidationPersistency : ISnapshotHourConsolidationPersistency
    {
        private readonly IMongoCollection<SnapshotWorkerStatusView> _hourlySnapshotCollection;
        private readonly ILogger _log;

        public SnapshotHourConsolidationPersistency(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("SnapshotHourConsolidationPersistency C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _hourlySnapshotCollection = database.GetCollection<SnapshotWorkerStatusView>("hourlySnapshotCollection");
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetHourlySnapshotAsync(IDateRange dateRange)
        {
            var snapshotViews = await _hourlySnapshotCollection.Find( h => h.DateRange.From >= dateRange.From && h.DateRange.To <= dateRange.To).ToListAsync<SnapshotWorkerStatusView>();
            return snapshotViews.Select(u => u.AsSnapshotWorkerStatus());
        }

        public async Task<bool> InsertManyAsync(int hour,IEnumerable<ISnapshotWorkerStatus> hourlyUptime)
        {
            try
            {
                var hourlySnapshotViews = hourlyUptime.Select(u => u.AsSnapshotWorkerStatusView());
                await _hourlySnapshotCollection.InsertManyAsync(hourlySnapshotViews);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }
    }
}
