using PoolScraper.Model.PowerPool;

namespace PoolScraper.Model
{
    public class WorkerUptimeHistory  : IWorkerUptimeHistory
    {
        public WorkerUptimeHistory(DateTime timestamp, bool isActive)
        {
            Timestamp = timestamp;
            IsActive = isActive;
        }
        public DateTime Timestamp { get; }
        public bool IsActive { get; }
    }
}