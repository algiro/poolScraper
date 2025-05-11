using log4net;
using MongoDB.Driver;
using PoolScraper.Components.Pages;
using PoolScraper.Domain;
using PoolScraper.Model;

namespace PoolScraper.Persistency
{
    public class WorkerPersistency : IWorkerPersistency
    {
        private readonly IMongoCollection<WorkerReadModel> _workerCollection;
        private readonly IMongoCollection<DisabledWorkerReadModel> _disabledWorkerCollection;

        private readonly ILogger _log;
        private readonly IMongoDatabase _database;
        public WorkerPersistency(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _workerCollection = _database.GetCollection<WorkerReadModel>("workers");
            _disabledWorkerCollection = _database.GetCollection<DisabledWorkerReadModel>("disabled_workers");

        }

        public async Task<IEnumerable<IWorker>> GetAllWorkerAsync()
        {
            var workersReadModel = await _workerCollection.Find(_ => true).SortBy(w => w.Id).ToListAsync<WorkerReadModel>();
            return workersReadModel.Select(w => w.AsWorker());
        }

        public async Task<IEnumerable<IDisabledWorker>> GetDisabledWorkersAsync()
        {
            var disabledWorkersReadModel = await _disabledWorkerCollection.Find<DisabledWorkerReadModel>(_ => true).ToListAsync();
            return disabledWorkersReadModel.Select(w => w.AsDisabledWorker());
        }

        public async Task<bool> InsertManyAsync(IEnumerable<IWorker> workers)
        {
            try
            {
                await _workerCollection.InsertManyAsync(workers.AsWorkersReadModel());
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveAllAsync()
        {
            try
            {
                await _database.DropCollectionAsync("workers");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error dropping collection {message}: {stackTrace}", ex.Message, ex.StackTrace);
                return false;
            }
        }
        public async Task<bool> ToggleEnableWorkerAsync(IWorkerId workerId)
        {
            var filter = Builders<DisabledWorkerReadModel>.Filter.And(
                Builders<DisabledWorkerReadModel>.Filter.Eq(x => x.PoolId, workerId.PoolId),
                Builders<DisabledWorkerReadModel>.Filter.Eq(x => x.Id, workerId.Id));

            if (await _disabledWorkerCollection.Find(filter).AnyAsync())
            {
                _log.LogInformation("Worker {workerId} already disabled in pool {poolId}, re-enabled it", workerId.Id, workerId.PoolId);
                await _disabledWorkerCollection.DeleteOneAsync(filter);
                return true;
            }
            else
            {
                _log.LogInformation("Disabling Worker {workerId} in pool {poolId}", workerId.Id, workerId.PoolId);
                await _disabledWorkerCollection.InsertOneAsync(new DisabledWorkerReadModel(workerId.PoolId, workerId.Id));
                return true;
            }

        }

    }

}
