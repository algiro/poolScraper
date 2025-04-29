using PoolScraper.Model.PowerPool;

namespace PoolScraper.Model.Scheduler
{
    public class PowerPoolScraperInfo
    {
        private PowerPoolUser userData;

        public PowerPoolScraperInfo(PowerPoolUser userData)
        {
            this.userData = userData;
        }
        public DateTime FetchedAt => userData.FetchedAt;
        public IEnumerable<(string algo, int count)> TotalWorkersAlgoCount => userData.GetTotalWorkersAlgoCount();
        public int WorkersCount => userData.GetTotalWorkersCount();
    }
}
