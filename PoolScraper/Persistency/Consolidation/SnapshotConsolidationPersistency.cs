using MongoDB.Driver;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model;
using log4net;
using PoolScraper.View;
using CommonUtils.Utils;
using PoolScraper.Components.Pages;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public class SnapshotConsolidationPersistency : ISnapshotConsolidationPersistency
    {
        private readonly IMongoCollection<SnapshotWorkerStatusReadModel> _consolidatedSnapshotCollection;
        private readonly ILogger _log;
        private readonly Granularity _granularity;

        public SnapshotConsolidationPersistency(ILogger log, string connectionString, string databaseName, Granularity granularity)
        {
            _log = log;
            _log.LogInformation("SnapshotConsolidationPersistency C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _granularity = granularity;
            _consolidatedSnapshotCollection = database.GetCollection<SnapshotWorkerStatusReadModel>(granularity.GetDBCollectionName());
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotAsync(IDateRange dateRange)
        {
            var snapshotViews = await _consolidatedSnapshotCollection
                .Find( h => h.Granularity == _granularity &&  h.DateRange.From >= dateRange.From && h.DateRange.To <= dateRange.To)
                .ToListAsync<SnapshotWorkerStatusReadModel>();
            return snapshotViews.Select(u => u.AsSnapshotWorkerStatus());
        }

        public async Task<bool> InsertManyAsync(IEnumerable<ISnapshotWorkerStatus> consolidatedSnapshots)
        {
            try
            {
                var snapshotStatusViews = consolidatedSnapshots.Select(u => u.AsSnapshotWorkerStatusView());
                await _consolidatedSnapshotCollection.InsertManyAsync(snapshotStatusViews);
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
