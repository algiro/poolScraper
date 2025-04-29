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
    public class UptimeHourConsolidationPersistency : IUptimeHourConsolidationPersistency
    {
        private readonly IMongoCollection<UptimePercentageView> _hourlyUptimeCollection;
        private readonly ILogger _log;

        public UptimeHourConsolidationPersistency(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("WorkerStatusDayHourConsolidationPersistency C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _hourlyUptimeCollection = database.GetCollection<UptimePercentageView>("hourlyUptimeCollection");
        }

        public async Task<IEnumerable<IUptimePercentage>> GetHourlyUptimeAsync(DateOnly dateOnly)
        {
            var dayStart = dateOnly.GetBeginOfDay();
            var dayEnd = dateOnly.GetEndOfDay();

            var uptimePercViews = await _hourlyUptimeCollection.Find( h => h.DateRange.From >= dayStart && h.DateRange.To <= dayEnd).ToListAsync<UptimePercentageView>();
            return uptimePercViews.Select(u => u.AsUptimePercentage());
        }

        public async Task<bool> InsertManyAsync(int hour,IEnumerable<IUptimePercentage> hourlyUptime)
        {
            try
            {
                var hourlyUptimeViews = hourlyUptime.Select(u => u.AsUptimePercentageView(Granularity.Hours));
                await _hourlyUptimeCollection.InsertManyAsync(hourlyUptimeViews);
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
