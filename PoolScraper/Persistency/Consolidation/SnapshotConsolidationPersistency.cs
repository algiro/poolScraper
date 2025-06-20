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
    public abstract class SnapshotConsolidationPersistency : ISnapshotConsolidationPersistency
    {
        private readonly IMongoCollection<SnapshotWorkerStatusReadModel> _consolidatedSnapshotCollection;
        private readonly ILogger _log;
        private readonly ISnapshotDataConsolidationPersistency _snapshotDataConsolidationPersistency;
        public abstract Granularity Granularity { get; }
        public SnapshotConsolidationPersistency(ILogger log, IPoolScraperConfig poolScraperConfig, ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency)
        {
            _log = log;
            _log.LogInformation("SnapshotConsolidationPersistency C.tor Granualarity: {granularity} with connection string: {connectionString} and database name: {databaseName}", Granularity, poolScraperConfig.MongoConnectionString  , poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            var database = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
            _snapshotDataConsolidationPersistency = snapshotDataConsolidationPersistency;
            _consolidatedSnapshotCollection = database.GetCollection<SnapshotWorkerStatusReadModel>(Granularity.GetDBCollectionName());
        }

        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotAsync(IDateRange dateRange)
        {
            _log.LogInformation($"GetSnapshotAsync {Granularity} for dataRange: {dateRange}");

            var snapshotViews = await _consolidatedSnapshotCollection
                .Find( h => h.Granularity == Granularity &&  h.DateRange.From >= dateRange.From && h.DateRange.To <= dateRange.To)
                .ToListAsync<SnapshotWorkerStatusReadModel>();
            _log.LogInformation($"GetSnapshotAsync granularity: {Granularity} for dataRange: {dateRange} snapshotViews#:{snapshotViews.Count()}");

            return snapshotViews.Select(u => u.AsSnapshotWorkerStatus());
        }

        public async Task<bool> InsertManyAsync(IEnumerable<ISnapshotWorkerStatus> consolidatedSnapshots,int sourceCount)
        {
            try
            {
                var snapshotStatusViews = consolidatedSnapshots.Select(u => u.AsSnapshotWorkerStatusView());
                await _consolidatedSnapshotCollection.InsertManyAsync(snapshotStatusViews);
                var refSnap = consolidatedSnapshots.First();
                await _snapshotDataConsolidationPersistency.InsertAsync(SnapshotDataConsolidationInfo.Create(Granularity, refSnap.DateRange, sourceCount));

                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }

        public async Task<(bool isSuccesfull,long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange)
        {
            try
            {
                var deleteResult = await _consolidatedSnapshotCollection.DeleteManyAsync(c => c.DateRange.From.Equals(dateRange.From) && c.DateRange.To.Equals(dateRange.To) 
                                                                                          && c.Granularity == Granularity.Days);
                _log.LogInformation("RemoveDayConsolidationAsync: {dateRange} deleted count: {deleteCount} ack:{ack}", dateRange, deleteResult.DeletedCount, deleteResult.IsAcknowledged);
                if (deleteResult.IsAcknowledged)
                {
                    if (await _snapshotDataConsolidationPersistency.RemoveDataConsolidationInfoAsync(dateRange, Granularity))
                    {
                        return (true, deleteResult.DeletedCount);
                    }
                    else
                    {
                        _log.LogError("Error removing data consolidation info from MongoDB");
                        return (false, deleteResult.DeletedCount);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error RemoveDayConsolidationAsync from MongoDB: {message}", ex.Message);
            }
            return (false,0);
        }
    }
}
