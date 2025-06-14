using CommonUtils.Utils.Logs;
using PoolScraper.Domain;
using PoolScraper.Persistency;

namespace PoolScraper.Service.Store
{
    public class WorkerStore(ILogger<WorkerStore> logger) : IWorkerStore
    {
        private IEnumerable<IWorker> _allWorkers = Enumerable.Empty<IWorker>();
        private IEnumerable<IDisabledWorker> _disabledWorkers = Enumerable.Empty<IDisabledWorker>();
        private IEnumerable<IWorker> _enabledWorkers = Enumerable.Empty<IWorker>();
        private IWorkerIdMap _workerIdMap = WorkerIdMap.Create(new Dictionary<IExternalId, IWorkerId>());

        public IEnumerable<IWorker> GetAllWorker() => _allWorkers;

        public void UpdateStore(IEnumerable<IWorker>? workers = null, IEnumerable<IDisabledWorker>? disabledWorkers = null,IWorkerIdMap? workerIdMap = null)
        {
            if (workers != null) _allWorkers = workers.ToList();
            if (disabledWorkers != null) _disabledWorkers = disabledWorkers.ToList();
            if (workerIdMap != null) _workerIdMap = workerIdMap;

            _enabledWorkers = _allWorkers.Where(w => !_disabledWorkers.Any(d => d.WorkerId.Equals(w.WorkerId))).ToList();
        }

        private IEnumerable<IWorker> GetReferenceWorkers(bool excludeDisabled = true)
            => excludeDisabled ? _enabledWorkers : _allWorkers;


        public (IWorker? worker,bool isDisabled) GetById(IWorkerId workerId)
        {
            try
            {
                var refWorkers = GetReferenceWorkers();
                var worker = refWorkers.FirstOrDefault(w => w.WorkerId.Equals(workerId));
                if (worker == null)
                {
                    logger.LogOnce(LogLevel.Warning, $"Worker not found fetching from allWorkers#: {refWorkers.Count()} worker by id: {workerId}");
                    worker = Worker.UNKNOWN;
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
        public IEnumerable<IWorker> GetWorkerByAlgo(string algo) => GetReferenceWorkers().Where(w => w.Algorithm == algo);
        public IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel) => GetReferenceWorkers().Where(w => w.Model == workerModel);

        public override string ToString()
        {
            return GetReferenceWorkers() != null
                ? "WorkersCount: #:" + GetReferenceWorkers().Count()
                : "No workers available";
        }
        public IEnumerable<IDisabledWorker> GetDisabledWorker() => _disabledWorkers;
        public IWorkerIdMap GetWorkerIdMap() => _workerIdMap;

    }
}
