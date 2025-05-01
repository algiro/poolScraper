using PoolScraper.Model;
using PoolScraper.Persistency;

namespace PoolScraper.Service.Store
{
    public class WorkerStore : IWorkerStore
    {
        private IEnumerable<IWorker> _allWorkers;
        public WorkerStore(IWorkerPersistency workerPersistency)
        {
            _allWorkers = workerPersistency.GetAllWorkerAsync().Result.ToList();
        }
        public IEnumerable<IWorker> GetAllWorker() => _allWorkers;
        public IWorker? GetById(long id) => _allWorkers.FirstOrDefault(w => w.Id == id);
        public IEnumerable<IWorker> GetWorkerByAlgo(string algo) => _allWorkers.Where(w => w.Algorithm == algo);
        public IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel) => _allWorkers.Where(w => w.Model == workerModel);

    }
}
