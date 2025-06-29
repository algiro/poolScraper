namespace PoolScraper.Persistency.Utils
{
    public class Counter
    {
        public string Id { get; set; } = string.Empty; // Sequence identifier, e.g., "orderId"
        public long Value { get; set; } // Current counter value
    }

}
