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
    public class UptimeDataConsolidationPersistency : IUptimeDataConsolidationPersistency
    {
        private readonly IMongoCollection<UptimeDataConsolidationInfoReadModel> _consolidationInfoCollection;
        private readonly ILogger _log;

        public UptimeDataConsolidationPersistency(ILogger<UptimeDataConsolidationPersistency> log, IPoolScraperConfig poolScraperConfig)
        {
            _log = log;
            _log.LogInformation("UptimeDataConsolidationPersistency C.tor with connection string: {connectionString} and database name: {databaseName}", poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            var database = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
            _consolidationInfoCollection = database.GetCollection<UptimeDataConsolidationInfoReadModel>("uptimeConsolidationInfo");
        }

        public async Task<IEnumerable<IUptimeDataConsolidationInfo>> GetUptimeDataConsolidationInfoAsync(IDateRange dateRange)
        {
            _log.LogInformation("GetUptimeDataConsolidationInfoAsync called with date range: {dateRange}", dateRange);
            var uptimeCons = await _consolidationInfoCollection
                .Find(h => h.DateRange.From >= dateRange.From && h.DateRange.To <= dateRange.To)
                .ToListAsync<UptimeDataConsolidationInfoReadModel>();
            return uptimeCons.Select(u => u.AsUptimeDataConsolidationInfo());
        }

        public async Task<bool> InsertAsync(IUptimeDataConsolidationInfo uptimeDataConsInfo)
        {
            _log.LogInformation("InsertAsync called with uptimeDataConsInfo: {uptimeDataConsInfo}", uptimeDataConsInfo);
            try
            {
                var uptimeDataConsInfoRM = uptimeDataConsInfo.AsUptimeDataConsolidationInfoReadModel();
                await _consolidationInfoCollection.InsertOneAsync(uptimeDataConsInfoRM);
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
