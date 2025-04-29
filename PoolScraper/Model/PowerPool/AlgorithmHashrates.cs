namespace PoolScraper.Model.PowerPool
{
    public class AlgorithmHashrates
    {
        public HashRateInfo Scrypt { get; set; }
        public HashRateInfo Sha256 { get; set; }
        public HashRateInfo X11 { get; set; }
        public HashRateInfo Kheavyhash { get; set; }
        public HashRateInfo Eaglesong { get; set; }
        public HashRateInfo Blake2s { get; set; }
    }
}
