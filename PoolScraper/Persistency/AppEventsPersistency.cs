using CommonUtils.Utils;
using log4net;
using MongoDB.Driver;
using PoolScraper.Components.Pages;
using PoolScraper.Config;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Persistency.Utils;
using PoolScraper.Service.Store;
using PoolScraper.View;

namespace PoolScraper.Persistency
{
    public class AppEventsPersistency : IAppEventsPersistency
    {
        private readonly IMongoCollection<AppEventReadModel> _appEventCollection;

        private readonly ILogger _log;
        private readonly IMongoDatabase _database;

        private const string APP_EVENT_COLL = "appEvents";
        public AppEventsPersistency(ILogger<AppEventsPersistency> log, IPoolScraperConfig poolScraperConfig)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            _database = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
            _appEventCollection = _database.GetCollection<AppEventReadModel>(APP_EVENT_COLL);
        }

        public async Task<IEnumerable<IAppEvent>> GetEvents(IDateRange dateRange)
        {
            var dateFilter = Builders<AppEventReadModel>.Filter.And(
                Builders<AppEventReadModel>.Filter.Gte(a => a.TimeStamp, dateRange.From),
                Builders<AppEventReadModel>.Filter.Lte(a => a.TimeStamp, dateRange.To)
            );
            var events = await _appEventCollection.Find(dateFilter).SortByDescending(a => a.TimeStamp).ToListAsync<AppEventReadModel>();                       
            return events;
        }
        public async Task InsertAsync(IAppEvent appEvent)
        {
            var appEventReadModel = new AppEventReadModel(appEvent.TimeStamp,appEvent.Type,appEvent.Message);
            await _appEventCollection.InsertOneAsync(appEventReadModel);
            _log.LogInformation("App event {eventType} inserted at {timeStamp}", appEvent.Type, appEvent.TimeStamp);
        }
    }

}
