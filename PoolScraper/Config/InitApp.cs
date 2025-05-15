using PoolScraper.Domain;

namespace PoolScraper.Config
{
    public interface IInitApp
    {
        void Init();
    }
    public class InitApp(ILogger<InitApp> logger) : IInitApp
    {
        public void Init()
        {
            try
            {
                var allFarms = PoolScraperConfig.Instance.Farms;
                Farm.UpdateStore(allFarms);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Error initializing app: " + ex.Message + ":" + ex.StackTrace );
            }
        }
    }
}
