using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public class MinerData
    {
        public AlgorithmHashrates Hashrate { get; set; }
        public List<BalanceView> Balances { get; set; }
        public List<Payment> Payments { get; set; }
        [JsonProperty("earnings")]
        public MinerEarnings Earnings { get; set; }  // Changed from List<Earning> to MinerEarnings
       
        [JsonProperty("workers")]
        public AlgorithmWorkers Workers { get; set; }
    }
}
