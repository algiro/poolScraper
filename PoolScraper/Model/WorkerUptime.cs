using PoolScraper.Model.PowerPool;

namespace PoolScraper.Model
{
    public class WorkerUptime(IWorker worker, double uptimePercentage) : IWorkerUptime
    {
        public string Algorithm { get; } = worker.Algorithm;

        public long Id { get; } = worker.Id;

        public string Name { get; } = worker.Name;

        public string PoolId { get; } = worker.PoolId;

        public double UptimePercentage { get; } = uptimePercentage;
    }
}
