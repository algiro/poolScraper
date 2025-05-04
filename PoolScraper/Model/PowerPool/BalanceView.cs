namespace PoolScraper.Model.PowerPool
{
    public class BalanceView
    {
        public int Coin { get; set; }
        public string CoinTicker { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
