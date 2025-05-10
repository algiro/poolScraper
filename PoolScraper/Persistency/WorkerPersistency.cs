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
        private readonly ILogger _log;

        public WorkerPersistency(ILogger log, string connectionString, string databaseName)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", connectionString, databaseName);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _workerCollection = database.GetCollection<WorkerReadModel>("workers");
        }
        
        public async Task<IEnumerable<IWorker>> GetAllWorkerAsync()
        {
            var workersReadModel = await _workerCollection.Find(_ => true).SortBy(w => w.Id).ToListAsync<WorkerReadModel>();
            return workersReadModel.Select(w => w.AsWorker());
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
    }

}
