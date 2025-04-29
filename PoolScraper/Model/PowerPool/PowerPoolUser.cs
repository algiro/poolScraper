namespace PoolScraper.Model.PowerPool
{
    public class PowerPoolUser
    {
        public string Id { get; set; }
        public string ApiKey { get; set; }
        public Dictionary<string, MinerData> Miners { get; set; }
        public DateTime FetchedAt { get; set; }
    }

    public static class PowerPoolUserExtension
    {
        public static IEnumerable<AlgorithmWorkers> GetAllAlgoWorkers(this PowerPoolUser user) => user.Miners.Values.Select(m => m.Workers);
        
        public static int GetTotalWorkersCount(this PowerPoolUser user)
        {
            return user.GetAllAlgoWorkers().Sum(w => w.GetTotalWorkersCount());
        }
        public static IEnumerable<(string algo, int count)> GetTotalWorkersAlgoCount(this PowerPoolUser user)
        {
            var allAlgoWorkers = user.GetAllAlgoWorkers();
            return allAlgoWorkers
                .SelectMany(workers => workers.GetTotalWorkersAlgoCount())
                .GroupBy(item => item.algo)
                .Select(group => (group.Key, group.Sum(x => x.count)));
        }
    }
}
