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
    public class WorkerPersistency : IWorkerPersistency
    {
        private readonly IMongoCollection<WorkerReadModel> _workerCollection;
        private readonly IMongoCollection<DisabledWorkerReadModel> _disabledWorkerCollection;
        private readonly IMongoCollection<WorkerIdMatchReadModel> _workerIdMatchWorkerCollection;

        private readonly ILogger _log;
        private readonly IMongoDatabase _database;
        private readonly ISequenceGenerator _sequenceGenerator;
        private readonly IWorkerStore _workerStore;

        private const string WORKERS_COLL = "workers";
        private const string WORKER_ID_MATCHES_COLL = "workerIdMatches";
        public WorkerPersistency(ILogger<WorkerPersistency> log, IWorkerStore workerStore,IPoolScraperConfig poolScraperConfig)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            _database = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
            _workerCollection = _database.GetCollection<WorkerReadModel>(WORKERS_COLL);
            _disabledWorkerCollection = _database.GetCollection<DisabledWorkerReadModel>("disabled_workers");
            _workerIdMatchWorkerCollection = _database.GetCollection<WorkerIdMatchReadModel>(WORKER_ID_MATCHES_COLL);
            _sequenceGenerator = new SequenceGenerator(log, poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            _workerStore = workerStore;
        }

        public async Task<IEnumerable<IWorker>> GetAllWorkerAsync()
        {
            var workersReadModel = await _workerCollection.Find(_ => true).SortBy(w => w.Id).ToListAsync<WorkerReadModel>();
            var workers = workersReadModel.Select(w => w.AsWorker());
            _log.LogInformation("GetAllWorkerAsync called, workers count: {count}", workersReadModel.Count);
            _workerStore.UpdateStore(workers);
            return workers;
        }

        public IEnumerable<IWorkerIdMatch> GetAllWorkerIdMatch()
        {
            var workersIdMatchesReadModel = _workerIdMatchWorkerCollection.Find(_ => true).ToList();
            var workerIdMatches =  workersIdMatchesReadModel.Select(w => w.AsWorkerIdMatch());
            _workerStore.UpdateStore(workerIdMap: WorkerIdMap.Create(workerIdMatches.ToDictionary(w => w.ExternalId, w => w.WorkerId)));
            return workerIdMatches;
        }

        public async Task<IEnumerable<IDisabledWorker>> GetDisabledWorkersAsync()
        {
            var disabledWorkersReadModel = await _disabledWorkerCollection.Find<DisabledWorkerReadModel>(_ => true).ToListAsync();
            var disabledWorkers = disabledWorkersReadModel.Select(w => w.AsDisabledWorker());
            _workerStore.UpdateStore(disabledWorkers:disabledWorkers);

            return disabledWorkers;
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

                // reload everything to update the store
                var updatedWorkers = await GetAllWorkerAsync();
                _workerStore.UpdateStore(workers: updatedWorkers);
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
                var workerIdMap = _workerStore.GetWorkerIdMap();
                foreach (var matches in workerIdMatches)
                {
                    workerIdMap.AddWorkerId(matches.ExternalId, matches.WorkerId);
                }

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
                await _database.DropCollectionAsync(WORKERS_COLL);
                await _database.DropCollectionAsync(WORKER_ID_MATCHES_COLL);
                // reload everything to update the store   
                await GetAllWorkerAsync();
                GetAllWorkerIdMatch();

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

        public Task<bool> UpdateWorkerAsync(WorkerDTO workerToUpdate)
        {
            var filter = Builders<WorkerReadModel>.Filter.And(
                Builders<WorkerReadModel>.Filter.Eq(x => x.PoolId, workerToUpdate.WorkerId.PoolId),
                Builders<WorkerReadModel>.Filter.Eq(x => x.Id, workerToUpdate.WorkerId.Id));
            var update = Builders<WorkerReadModel>.Update
                .Set(x => x.Algorithm, workerToUpdate.Algorithm)
                .Set(x => x.Name, workerToUpdate.Name)
                .Set(x => x.NominalHashRate, workerToUpdate.NominalHashRate)
                .Set(x => x.Provider, workerToUpdate.Provider)
                .Set(x => x.ModelId, workerToUpdate.Model.Id)
                .Set(x => x.FarmId, workerToUpdate.Farm.Id);
            return _workerCollection.UpdateOneAsync(filter, update).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    _log.LogInformation("Worker {workerId} updated successfully", workerToUpdate.WorkerId);
                    return true;
                }
                else
                {
                    _log.LogError("Error updating Worker {workerId}: {error}", workerToUpdate.WorkerId, t.Exception?.Message);
                    return false;
                }
            });
        }
    }

}
