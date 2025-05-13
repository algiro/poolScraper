using PoolScraper.Domain;

namespace PoolScraper.Persistency
{
    public interface IWorkerPersistency
    {
        Task<IEnumerable<IWorker>> GetAllWorkerAsync();
        Task<IEnumerable<IDisabledWorker>> GetDisabledWorkersAsync(); 
        Task<bool> InsertManyAsync(IEnumerable<INewWorker> workers);
        Task<bool> ToggleEnableWorkerAsync(IWorkerId workerId);
        Task<bool> RemoveAllAsync();
        IEnumerable<IWorkerIdMatch> GetAllWorkerIdMatch();
        //Task<bool> InsertWorkerIdMatchAsync(IWorkerIdMatch workerIdMatch);
    }

}
