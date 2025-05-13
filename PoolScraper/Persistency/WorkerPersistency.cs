using CommonUtils.Utils;
using log4net;
using MongoDB.Driver;
using PoolScraper.Components.Pages;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Persistency.Utils;

namespace PoolScraper.Persistency
{
    public class WorkerPersistency : IWorkerPersistency
    {
        private readonly IMongoCollection<WorkerReadModel> _workerCollection;
        private readonly IMongoCollection<DisabledWorkerReadModel> _disabledWorkerCollection;
        private readonly IMongoCollection<WorkerIdMatchReadModel> _workerIdMatchWorkerCollection;

        private readonly ILogger _log;
        private readonly IMongoDatabase _database;
        private readonly ISequenceGenerator _sequenceGenerator;
        public WorkerPersistency(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _workerCollection = _database.GetCollection<WorkerReadModel>("workers");
            _disabledWorkerCollection = _database.GetCollection<DisabledWorkerReadModel>("disabled_workers");
            _workerIdMatchWorkerCollection = _database.GetCollection<WorkerIdMatchReadModel>("workerIdMatches");
            _sequenceGenerator = new SequenceGenerator(log, connectionString, databaseName);
        }

        public async Task<IEnumerable<IWorker>> GetAllWorkerAsync()
        {
            var workersReadModel = await _workerCollection.Find(_ => true).SortBy(w => w.Id).ToListAsync<WorkerReadModel>();
            return workersReadModel.Select(w => w.AsWorker());
        }

        public IEnumerable<IWorkerIdMatch> GetAllWorkerIdMatch()
        {
            var workersIdMatchesReadModel = _workerIdMatchWorkerCollection.Find(_ => true).ToList();
            return workersIdMatchesReadModel.Select(w => w.AsWorkerIdMatch());
        }

        public async Task<IEnumerable<IDisabledWorker>> GetDisabledWorkersAsync()
        {
            var disabledWorkersReadModel = await _disabledWorkerCollection.Find<DisabledWorkerReadModel>(_ => true).ToListAsync();
            return disabledWorkersReadModel.Select(w => w.AsDisabledWorker());
        }

        public async Task<bool> InsertManyAsync(IEnumerable<INewWorker> workers)
        {
            try
            {
                var workerWithUpdatedID = workers.Select(w =>
                {
                    var updatedWorkerId = _sequenceGenerator.GetNextSequence("workerIdSeq");
                    return w.UpdateId(updatedWorkerId);
                }).ToArray();

                await _workerCollection.InsertManyAsync(workerWithUpdatedID.AsWorkersReadModel());
                await InsertWorkerIdMatchAsync(workerWithUpdatedID.AsWorkerIdMatches());
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message}", ex.Message);
                return false;
            }
        }

        private async Task<bool> InsertWorkerIdMatchAsync(IEnumerable<IWorkerIdMatch> workerIdMatches)
        {
            try
            {
                await _workerIdMatchWorkerCollection.InsertManyAsync(workerIdMatches.AsReadModels());
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error inserting data into MongoDB: {message} {stackTrace}", ex.Message, ex.StackTrace);
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
