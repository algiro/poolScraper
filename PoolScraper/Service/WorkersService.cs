using MongoDB.Driver;
using PoolScraper.Domain;
using PoolScraper.Model;
using PoolScraper.Persistency;
using PoolScraper.View;

namespace PoolScraper.Service
{
    public class WorkersService : IWorkersService
    {

        private readonly ILogger _log;
        private readonly IWorkerPersistency _workerPersistency;
        public WorkersService(ILogger<WorkersService> log, IWorkerPersistency workerPersistency)
        {
            _log = log;
            _workerPersistency = workerPersistency;
            _log.LogInformation("WorkersService C.tor");

        }

        public async Task<bool> ToggleEnableWorkerAsync(IWorkerId workerId) => await _workerPersistency.ToggleEnableWorkerAsync(workerId);
        public async Task<IEnumerable<WorkerDTO>> GetWorkersAsync() 
        {
            try
            {
                var workers = await _workerPersistency.GetAllWorkerAsync();
                var disabledWorkers = await _workerPersistency.GetDisabledWorkersAsync();

                _log.LogInformation("GetWorkers # {count} Disabled # {disabled}", workers.Count(),disabledWorkers.Count());
                return workers.Select(w => w.ToWorkerDTO(disabledWorkers));
            }
            catch (Exception ex)
            {
                _log.LogError("Error fetching workers data : {message}", ex.Message);
            }
            return Enumerable.Empty<WorkerDTO>();
        }

        public async Task<bool> RemoveAllWorkerAsync()
        {
            try
            {
                await _workerPersistency.RemoveAllAsync();
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error removing all workers data : {message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateWorkerAsync(WorkerDTO workerToUpdate)
        {
            try
            {
                await _workerPersistency.UpdateWorkerAsync(workerToUpdate);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error UpdateWorkerAsync: {workerToUpdate} {message}", workerToUpdate,  ex.Message);
                return false;
            }

        }
    }
}
