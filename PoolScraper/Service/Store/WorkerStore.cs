using PoolScraper.Domain;
using PoolScraper.Persistency;

namespace PoolScraper.Service.Store
{
    public class WorkerStore(ILogger logger, IWorkerPersistency workerPersistency) : IWorkerStore
    {
        private IEnumerable<IWorker> _allWorkers = Enumerable.Empty<IWorker>();
        private IEnumerable<IDisabledWorker> _disabledWorkers = Enumerable.Empty<IDisabledWorker>();
        public IEnumerable<IWorker> GetAllWorker() => _allWorkers;

        public WorkerStore(ILogger logger, IEnumerable<IWorker> workers) : this (logger, (IWorkerPersistency) null)
        {
            _allWorkers = workers;
        }

        public async Task<IEnumerable<IWorker>> LoadAllWorkerAsync(bool excludeDisabled=true)
        {
            logger.LogInformation("GetAllWorkerAsync called");
            _allWorkers = (await workerPersistency.GetAllWorkerAsync()).ToList();
            _disabledWorkers = (await workerPersistency.GetDisabledWorkersAsync()).ToList();
            logger.LogInformation("GetAllWorkerAsync called, workers count: {count}, excluded: {disabledWorkersCount}", _allWorkers.Count(), _disabledWorkers.Count());

            if (excludeDisabled)
            {
                _allWorkers = _allWorkers.Where(w => !_disabledWorkers.Any(dw => dw.WorkerId == w.WorkerId)).ToList();
            }
            logger.LogInformation("GetAllWorkerAsync called, final workers count: {count}", _allWorkers.Count());
            return _allWorkers;
        }

        public (IWorker? worker,bool isDisabled) GetById(IWorkerId workerId)
        {
            try
            {
                var worker = _allWorkers.FirstOrDefault(w => w.WorkerId.Equals(workerId));
                if (worker == null)
                {
                    logger.LogWarning("Worker not found fetching from allWorkers#: {workerStore} worker by id: {id}", _allWorkers.Count(), workerId);
                }
                return (worker,_disabledWorkers.Any(d=> d.WorkerId.Equals(workerId)));
            }
            catch (Exception ex)
            {
                logger.LogError("Error fetching from allWorkers#: {workerStore} worker by id: {id}, error: {error}", _allWorkers.Count(), workerId, ex.Message);
                foreach (var worker in _allWorkers)
                {
                    logger.LogError("Worker: {worker}", worker.ToString());
                }
                return (null,true);
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
