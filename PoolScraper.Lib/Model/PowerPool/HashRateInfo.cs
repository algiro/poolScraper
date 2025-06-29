namespace PoolScraper.Model.PowerPool
{
    public class HashRateInfo
    {
        public double Hashrate { get; set; }
        public string HashrateUnits { get; set; } = string.Empty;
        public double HashrateAvg { get; set; }
        public string HashrateAvgUnits { get; set; } = string.Empty;
    }
}
