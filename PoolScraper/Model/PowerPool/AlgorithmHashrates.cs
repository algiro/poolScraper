namespace PoolScraper.Model.PowerPool
{
    public class AlgorithmHashrates
    {
        public HashRateInfo Scrypt { get; set; } = new HashRateInfo();
        public HashRateInfo Sha256 { get; set; } = new HashRateInfo();
        public HashRateInfo X11 { get; set; } = new HashRateInfo();
        public HashRateInfo Kheavyhash { get; set; } = new HashRateInfo();
        public HashRateInfo Eaglesong { get; set; } = new HashRateInfo();
        public HashRateInfo Blake2s { get; set; } = new HashRateInfo();
    }
}
