using PoolScraper.Domain;

namespace PoolScraper.Service.Store
{
    public interface IWorkerStore
    {
        Task<IEnumerable<IWorker>> LoadAllWorkerAsync(bool excludeDisabled);
        (IWorker? worker, bool isDisabled) GetById(long id);
        IEnumerable<IWorker> GetAllWorker();
        IEnumerable<IWorker> GetWorkerByAlgo(string algo);
        IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel);
    }
}
