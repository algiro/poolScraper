using PoolScraper.Domain;

namespace PoolScraper.Service.Store
{
    public interface IWorkerStore
    {
        Task<IEnumerable<IWorker>> LoadAllWorkerAsync();
        IWorker? GetById(long id);
        IEnumerable<IWorker> GetAllWorker();
        IEnumerable<IWorker> GetWorkerByAlgo(string algo);
        IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel);
    }
}
