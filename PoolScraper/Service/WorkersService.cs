using CommonUtils.Utils;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.View;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PoolScraper.Service
{
    public class WorkersService : IWorkersService
    {
        private readonly IMongoCollection<Worker> _workerCollection;
        private readonly IMongoCollection<DisabledWorker> _disabledWorkerCollection;

        private readonly ILogger _log;
        public WorkersService(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _workerCollection = database.GetCollection<Worker>("workers");
            _disabledWorkerCollection = database.GetCollection<DisabledWorker>("disabled_workers");

        }

        public async Task<bool> ToggleEnableWorkerAsync(string poolId, long workerId)
        {
            var filter = Builders<DisabledWorker>.Filter.And(
                Builders<DisabledWorker>.Filter.Eq(x => x.PoolId, poolId),
                Builders<DisabledWorker>.Filter.Eq(x => x.Id, workerId));

            if (await _disabledWorkerCollection.Find(filter).AnyAsync())
            {
                _log.LogInformation("Worker {workerId} already disabled in pool {poolId}, re-enabled it", workerId, poolId);
                await _disabledWorkerCollection.DeleteOneAsync(filter);
                return true;
            }
            else
            {
                _log.LogInformation("Disabling Worker {workerId} in pool {poolId}", workerId, poolId);
                await _disabledWorkerCollection.InsertOneAsync(new DisabledWorker(poolId, workerId));
                return true;
            }

        }

        public async Task<IEnumerable<WorkerDTO>> GetWorkersAsync()
        {
            try
            {
                var workers = await _workerCollection.Find<Worker>(_ => true).ToListAsync();
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
