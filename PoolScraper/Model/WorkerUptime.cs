namespace PoolScraper.Model
{
    public class WorkerUptime(IWorker worker, double uptimePercentage) : IWorkerUptime
    {
        public string Algorithm { get; } = worker.Algorithm;

        public long Id { get; } = worker.Id;

        public string Name { get; } = worker.Name;

        public string PoolId { get; } = worker.PoolId;

        public double UptimePercentage { get; } = uptimePercentage;

        public WorkerModel Model { get; } = worker.Model;

        public Farm FarmId { get; } = worker.FarmId;
    }
}
