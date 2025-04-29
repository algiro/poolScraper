using PoolScraper.Model.PowerPool;

namespace PoolScraper.Model
{
    public interface IWorkerUptimeHistory 
    {
        DateTime Timestamp { get; }
        bool IsActive { get; }
    }
}