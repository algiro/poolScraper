using PoolScraper.Domain;

namespace PoolScraper.Persistency
{
    public interface IWorkerPersistency
    {
        Task<IEnumerable<IWorker>> GetAllWorkerAsync();
        Task<bool> InsertManyAsync(IEnumerable<IWorker> workers);
    }

}
