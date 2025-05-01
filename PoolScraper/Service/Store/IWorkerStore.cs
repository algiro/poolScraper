using PoolScraper.Model;

namespace PoolScraper.Service.Store
{
    public interface IWorkerStore
    {
        IWorker? GetById(long id);
        IEnumerable<IWorker> GetAllWorker();
        IEnumerable<IWorker> GetWorkerByAlgo(string algo);
        IEnumerable<IWorker> GetWorkerByModel(WorkerModel workerModel);
    }
}
