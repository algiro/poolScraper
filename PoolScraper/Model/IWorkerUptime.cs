namespace PoolScraper.Model
{
    public interface IWorkerUptime : IWorker
    {
        double UptimePercentage { get; }
    }
}