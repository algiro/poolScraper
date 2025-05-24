using PoolScraper.Domain;
using PoolScraper.Persistency;

namespace PoolScraper.Config
{
    public interface IInitApp
    {
        Task InitAsync();
    }
    public class InitApp(ILogger<InitApp> logger, IWorkerPersistency workerPersistency) : IInitApp
    {
        public async Task InitAsync()
        {
            try
            {
                logger.LogInformation("Initializing application...");
                var allFarms = PoolScraperConfig.Instance.Farms;
                Farm.UpdateStore(allFarms);
                // Load all workers and their status to initialize the worker store.
                await workerPersistency.GetAllWorkerIdMatchAsync();
                await workerPersistency.GetDisabledWorkersAsync();
                await workerPersistency.GetAllWorkerAsync();
                logger.LogInformation("Initializing application...DONE");

            }
            catch (Exception ex)
            {
                logger.LogWarning("Error initializing app: " + ex.Message + ":" + ex.StackTrace );
            }
        }
    }
}
