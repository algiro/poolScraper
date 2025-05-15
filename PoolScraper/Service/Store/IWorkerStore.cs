using PoolScraper.Domain;

namespace PoolScraper.Service.Store
{
    public interface IWorkerStore
    {
        void UpdateStore(IEnumerable<IWorker>? workers = null, IEnumerable<IDisabledWorker>? disabledWorkers = null, IWorkerIdMap? workerIdMap = null);
        (IWorker? worker, bool isDisabled) GetById(IWorkerId id);
        IEnumerable<IWorker> GetAllWorker();
        IEnumerable<IDisabledWorker> GetDisabledWorker();
        IWorkerIdMap GetWorkerIdMap();
        IEnumerable<IWorker> GetWorkerByAlgo(string algo);
        IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel);
    }
}
