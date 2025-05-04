using PoolScraper.Model.PowerPool;

namespace PoolScraper.Domain
{
    public interface IWorkerUptimeHistory 
    {
        DateTime Timestamp { get; }
        bool IsActive { get; }
    }
    public static class WorkerUptimeHistory
    {
        public static IWorkerUptimeHistory Create(DateTime timestamp, bool isActive) => new DefaultWorkerUptimeHistory(timestamp, isActive);
        private class DefaultWorkerUptimeHistory : IWorkerUptimeHistory
        {
            public DefaultWorkerUptimeHistory(DateTime timestamp, bool isActive)
            {
                Timestamp = timestamp;
                IsActive = isActive;
            }
            public DateTime Timestamp { get; }
            public bool IsActive { get; }
        }
    }
}