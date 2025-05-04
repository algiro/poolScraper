using MongoDB.Driver;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.View;

namespace PoolScraper.Service
{
    public class WorkersService : IWorkersService
    {
        private readonly IMongoCollection<WorkerReadModel> _workerCollection;
        private readonly IMongoCollection<DisabledWorker> _disabledWorkerCollection;

        private readonly ILogger _log;
        public WorkersService(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _workerCollection = database.GetCollection<WorkerReadModel>("workers");
            _disabledWorkerCollection = database.GetCollection<DisabledWorker>("disabled_workers");

        }

        public async Task<bool> ToggleEnableWorkerAsync(IWorkerId workerId)
        {
            var filter = Builders<DisabledWorker>.Filter.And(
                Builders<DisabledWorker>.Filter.Eq(x => x.PoolId, workerId.PoolId),
                Builders<DisabledWorker>.Filter.Eq(x => x.Id, workerId.Id));

            if (await _disabledWorkerCollection.Find(filter).AnyAsync())
            {
                _log.LogInformation("Worker {workerId} already disabled in pool {poolId}, re-enabled it", workerId.Id, workerId.PoolId);
                await _disabledWorkerCollection.DeleteOneAsync(filter);
                return true;
            }
            else
            {
                _log.LogInformation("Disabling Worker {workerId} in pool {poolId}", workerId.Id, workerId.PoolId);
                await _disabledWorkerCollection.InsertOneAsync(new DisabledWorker(workerId.PoolId, workerId.Id));
                return true;
            }

        }

        public async Task<IEnumerable<WorkerDTO>> GetWorkersAsync()
        {
            try
            {
                var workers = await _workerCollection.Find<WorkerReadModel>(_ => true).ToListAsync();
                var disabledWorkers = await _disabledWorkerCollection.Find<DisabledWorker>(_ => true).ToListAsync();

                _log.LogInformation("GetWorkers # {count} Disabled # {disabled}", workers.Count(),disabledWorkers.Count());
                return workers.Select(w => w.ToWorkerDTO(disabledWorkers));
            }
            catch (Exception ex)
            {
                _log.LogError("Error fetching workers data : {message}", ex.Message);
            }
            return Enumerable.Empty<WorkerDTO>();
        }
    }
}
