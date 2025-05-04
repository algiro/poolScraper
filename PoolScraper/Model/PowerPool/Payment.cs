namespace PoolScraper.Model.PowerPool
{
    public class Payment
    {
        public string Address { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public double Timestamp { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public string Txid { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
