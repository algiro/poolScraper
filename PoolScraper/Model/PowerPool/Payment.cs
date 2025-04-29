namespace PoolScraper.Model.PowerPool
{
    public class Payment
    {
        public string Address { get; set; }
        public decimal Value { get; set; }
        public double Timestamp { get; set; }
        public string Ticker { get; set; }
        public string Txid { get; set; }
        public DateTime Date { get; set; }
    }
}
