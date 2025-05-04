namespace PoolScraper.Model.PowerPool
{
    public class AlgorithmEarnings
    {
        public string EarningTimestamp { get; set; } = string.Empty;
        public List<Coin> Coins { get; set; } = new List<Coin>();
        public decimal UsdValue { get; set; }
        public string Source { get; set; } = string.Empty;
        public double Speed { get; set; }
        public string SpeedUnits { get; set; } = string.Empty;
    }
}
