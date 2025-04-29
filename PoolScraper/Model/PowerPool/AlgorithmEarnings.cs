namespace PoolScraper.Model.PowerPool
{
    public class AlgorithmEarnings
    {
        public string EarningTimestamp { get; set; }
        public List<Coin> Coins { get; set; }
        public decimal UsdValue { get; set; }
        public string Source { get; set; }
        public double Speed { get; set; }
        public string SpeedUnits { get; set; }
    }
}
