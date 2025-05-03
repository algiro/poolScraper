using PoolScraper.Model;
using PoolScraper.Model.PowerPool;
using PoolScraper.View;

namespace PoolScraper.Service
{
    public interface IWorkersService
    {
        Task<IEnumerable<WorkerDTO>> GetWorkersAsync();
        Task<bool> ToggleEnableWorkerAsync(IWorkerId workerId);
    }
}