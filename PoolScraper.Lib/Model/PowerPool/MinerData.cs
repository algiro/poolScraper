using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public class MinerData
    {
        public AlgorithmHashrates Hashrate { get; set; } = new AlgorithmHashrates();
        public List<BalanceView> Balances { get; set; } = new List<BalanceView>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
        [JsonProperty("earnings")]
        public MinerEarnings Earnings { get; set; }  = new MinerEarnings();

        [JsonProperty("workers")]
        public AlgorithmWorkers Workers { get; set; } = new AlgorithmWorkers();
    }
}
