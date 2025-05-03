using PoolScraper.Model;
using PoolScraper.Persistency;

namespace PoolScraper.Service.Store
{
    public class WorkerStore(ILogger<WorkerStore> logger, IWorkerPersistency workerPersistency) : IWorkerStore
    {
        private IEnumerable<IWorker> _allWorkers = Enumerable.Empty<IWorker>();
        public IEnumerable<IWorker> GetAllWorker() => _allWorkers;


        public async Task<IEnumerable<IWorker>> LoadAllWorkerAsync()
        {
            logger.LogInformation("GetAllWorkerAsync called");
            _allWorkers = (await workerPersistency.GetAllWorkerAsync()).ToList();
            logger.LogInformation("GetAllWorkerAsync called, workers count: {count}", _allWorkers.Count());
            return _allWorkers;
        }

        public IWorker? GetById(long id) => _allWorkers.FirstOrDefault(w => w.WorkerId.Id == id);
        public IEnumerable<IWorker> GetWorkerByAlgo(string algo) => _allWorkers.Where(w => w.Algorithm == algo);
        public IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel) => _allWorkers.Where(w => w.Model == workerModel);

        public override string ToString()
        {
            return _allWorkers != null
                ? "WorkersCount: #:" + _allWorkers.Count()
                : "No workers available";
        }
    }
}
