using CommonUtils.Utils;
using log4net;
using MongoDB.Driver;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency.Consolidation;
using System.Net.Http;

namespace PoolScraper.Persistency
{
    public class PowerPoolScrapingPersistency : IPowerPoolScrapingPersistency
    {
        private readonly IMongoCollection<PowerPoolUser> _scrapingCollection;
        private readonly ILogger _log;
        private readonly IPool powerPool = Pool.CreatePowerPool();
        private readonly IWorkerIdMap _workerIdMap;
        public PowerPoolScrapingPersistency(ILogger log, string connectionString, string databaseName, IWorkerIdMap workerIdMap)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _scrapingCollection = database.GetCollection<PowerPoolUser>("powerPoolUsers");
            _workerIdMap = workerIdMap;
        }

        public async Task<bool> InsertAsync(PowerPoolUser powerPoolUser)
        {
            try
            {
                await _scrapingCollection.InsertOneAsync(powerPoolUser);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }

        public async Task<PowerPoolUser> GetLatestUserDataAsync()
        {
            var filter = Builders<PowerPoolUser>.Filter.Eq(u => u.ApiKey, powerPool.ApiKey);
            return await _scrapingCollection.Find(filter)
                .SortByDescending(u => u.FetchedAt)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<PowerPoolUser>> GetDataRangeAsync(DateTime from, DateTime to)
        {
            var filter = Builders<PowerPoolUser>.Filter.And(
                Builders<PowerPoolUser>.Filter.Gte(x => x.FetchedAt, from),
                Builders<PowerPoolUser>.Filter.Lte(x => x.FetchedAt, to)
            );
            var result = await _scrapingCollection.Find(filter)
                .SortBy(x => x.FetchedAt)
                .ToListAsync();

            return result.AsEnumerable();
        }
        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotWorkerStatusAsync(IWorkerId workerId, DateTime from, DateTime to)
        {
            _log.LogInformation("GetSnapshotWorkerStatusAsync called with workerId: {workerId}, from: {from}, to: {to}", workerId.Id, from, to);
            var filter = Builders<PowerPoolUser>.Filter.And(
                Builders<PowerPoolUser>.Filter.Gte(x => x.FetchedAt, from),
                Builders<PowerPoolUser>.Filter.Lte(x => x.FetchedAt, to)
            );
            var result = await _scrapingCollection.Find(filter)
                .SortBy(x => x.FetchedAt)
                .ToListAsync();
            _log.LogInformation("GetSnapshotWorkerStatusAsync not filtered by {workerId} #", result.Count());

            var workerStatus = result.AsSnapshotWorkerStatus(_workerIdMap);
            _log.LogInformation("GetSnapshotWorkerStatusAsync transformed as workerStatus {workerId} #", workerStatus.Count());
            return workerStatus.Where(w => w.WorkerId.Id == workerId.Id);

        }
    }
}
