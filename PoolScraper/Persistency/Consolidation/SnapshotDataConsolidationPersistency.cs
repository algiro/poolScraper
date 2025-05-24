using MongoDB.Driver;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model;
using log4net;
using PoolScraper.View;
using CommonUtils.Utils;
using PoolScraper.Components.Pages;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Config;

namespace PoolScraper.Persistency.Consolidation
{
    public class SnapshotDataConsolidationPersistency : ISnapshotDataConsolidationPersistency
    {
        private readonly IMongoCollection<SnapshotDataConsolidationInfoReadModel> _consolidationInfoCollection;
        private readonly ILogger _log;

        public SnapshotDataConsolidationPersistency(ILogger<SnapshotDataConsolidationPersistency> log, IPoolScraperConfig poolScraperConfig)
        {
            _log = log;
            _log.LogInformation("SnapshotConsolidationPersistency C.tor with connection string: {connectionString} and database name: {databaseName}", poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            var database = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
            _consolidationInfoCollection = database.GetCollection<SnapshotDataConsolidationInfoReadModel>("snapshotConsolidationInfo");
        }

        public async Task<IEnumerable<ISnapshotDataConsolidationInfo>> GetSnapshotDataConsolidationInfoAsync(IDateRange dateRange)
        {
            _log.LogInformation("GetSnapshotDataConsolidationInfoAsync called with date range: {dateRange}", dateRange);
            var snapshotViews = await _consolidationInfoCollection
                .Find(h => h.DateRange.From >= dateRange.From && h.DateRange.To <= dateRange.To)
                .ToListAsync<SnapshotDataConsolidationInfoReadModel>();
            return snapshotViews.Select(u => u.AsSnapshotDataConsolidationInfo());
        }

        public async Task<bool> InsertAsync(ISnapshotDataConsolidationInfo snapshotDataConsolidationInfo)
        {
            _log.LogInformation("InsertAsync called with snapshotDataConsolidationInfo: {snapshotDataConsolidationInfo}", snapshotDataConsolidationInfo);
            try
            {
                var snapshotDataConsInfo = snapshotDataConsolidationInfo.AsSnapshotDataConsolidationInfoReadModel();
                await _consolidationInfoCollection.InsertOneAsync(snapshotDataConsInfo);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveDataConsolidationInfoAsync(IDateRange dateRange, Granularity granularity)
        {
            try
            {
                var deleteResult = await _consolidationInfoCollection.DeleteManyAsync(c => c.DateRange.From.Equals(dateRange.From) && c.DateRange.To.Equals(dateRange.To)
                                                                                          && c.Granularity == granularity.ToString());
                _log.LogInformation("RemoveDataConsolidationInfoAsync: {dateRange} deleted count: {deleteCount}", dateRange, deleteResult.DeletedCount);
                return (true);
            }
            catch (Exception ex)
            {
                _log.LogError("Error RemoveDataConsolidationInfoAsync from MongoDB: {message}", ex.Message);
                return false;
            }

        }
    }
}
