using CommonUtils.Utils;
using log4net;
using MongoDB.Driver;
using PoolScraper.Components.Pages;
using PoolScraper.Config;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service.Store;
using PoolScraper.View;

namespace PoolScraper.Persistency.Consolidation
{
    public class UptimeDailyConsolidationPersistency : IUptimeDailyConsolidationPersistency
    {
        private readonly IMongoCollection<UptimePercentageReadModel> _dailyUptimeCollection;
        private readonly ILogger _log;
        private readonly IUptimeDataConsolidationPersistency _uptimeDataConsolidationPersistency;
        public UptimeDailyConsolidationPersistency(ILogger<PowerPoolScrapingPersistency> log, IPoolScraperConfig poolScraperConfig, IUptimeDataConsolidationPersistency uptimeDataConsolidationPersistency)
        {
            _log = log;
            _log.LogInformation("UptimeDailyConsolidationPersistency C.tor with connection string: {connectionString} and database name: {databaseName}", poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            var database = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
            _uptimeDataConsolidationPersistency = uptimeDataConsolidationPersistency;
            _dailyUptimeCollection = database.GetCollection<UptimePercentageReadModel>("dailyyUptimeCollection");
        }

        public async Task<IEnumerable<IUptimePercentage>> GetDailyUptimeAsync(DateOnly dateOnly)
        {
            var dayStart = dateOnly.GetBeginOfDay();
            var dayEnd = dateOnly.GetEndOfDay();

            var uptimePercViews = await _dailyUptimeCollection.Find( h => h.DateRange.From >= dayStart && h.DateRange.To <= dayEnd).ToListAsync<UptimePercentageReadModel>();
            return uptimePercViews.Select(u => u.AsUptimePercentage());
        }

        public async Task<bool> InsertManyAsync(DateOnly date, IEnumerable<IUptimePercentage> dailyUptimes)
        {
            try
            {
                var dailyUptimeViews = dailyUptimes.Select(u => u.AsUptimePercentageView(Granularity.Days));
                await _dailyUptimeCollection.InsertManyAsync(dailyUptimeViews);
                var refUptime = dailyUptimes.First();
                var uptimesCount = dailyUptimes.Count();
                await _uptimeDataConsolidationPersistency.InsertAsync(UptimeDataConsolidationInfo.Create(Granularity.Days, refUptime.DateRange, uptimesCount));

                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("InsertManyAsync Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }

        public async Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange)
        {
            try
            {
                var deleteResult = await _dailyUptimeCollection.DeleteManyAsync(c => c.DateRange.From.Equals(dateRange.From) && c.DateRange.To.Equals(dateRange.To)
                                                                                          && c.Granularity == Granularity.Days);
                _log.LogInformation("RemoveDayConsolidationAsync: {dateRange} deleted count: {deleteCount} ack:{ack}", dateRange, deleteResult.DeletedCount, deleteResult.IsAcknowledged);
                if (deleteResult.IsAcknowledged)
                {
                    if (await _uptimeDataConsolidationPersistency.RemoveDataConsolidationInfoAsync(dateRange, Granularity.Days))
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
            return (false, 0);
        }
    }
}
