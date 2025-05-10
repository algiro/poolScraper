using PoolScraper.Domain;
using PoolScraper.Persistency;

namespace PoolScraper.Service.Store
{
    public class WorkerStore(ILogger logger, IWorkerPersistency workerPersistency) : IWorkerStore
    {
        private IEnumerable<IWorker> _allWorkers = Enumerable.Empty<IWorker>();
        public IEnumerable<IWorker> GetAllWorker() => _allWorkers;

        public WorkerStore(ILogger logger, IEnumerable<IWorker> workers) : this (logger, (IWorkerPersistency) null)
        {
            _allWorkers = workers;
        }

        public async Task<IEnumerable<IWorker>> LoadAllWorkerAsync()
        {
            logger.LogInformation("GetAllWorkerAsync called");
            _allWorkers = (await workerPersistency.GetAllWorkerAsync()).ToList();
            logger.LogInformation("GetAllWorkerAsync called, workers count: {count}", _allWorkers.Count());
            return _allWorkers;
        }

        public IWorker? GetById(long id)
        {
            try
            {
                var worker = _allWorkers.FirstOrDefault(w => w.WorkerId.Id == id);
                if (worker == null)
                {
                    logger.LogWarning("Worker not found fetching from allWorkers#: {workerStore} worker by id: {id}", _allWorkers.Count(), id);
                }
                return worker;
            }
            catch (Exception ex)
            {
                logger.LogError("Error fetching from allWorkers#: {workerStore} worker by id: {id}, error: {error}", _allWorkers.Count(), id, ex.Message);
                foreach (var worker in _allWorkers)
                {
                    logger.LogError("Worker: {worker}", worker.ToString());
                }
                return null;
            }
        }
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
