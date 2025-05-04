namespace PoolScraper.Model.PowerPool
{
    public class Coin
    {
        public int CoinId { get; set; }
        public string CoinTicker { get; set; } = string.Empty;
        public decimal CoinBalance { get; set; }
        public int CoinDecimals { get; set; }
    }
}
