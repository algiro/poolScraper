namespace PoolScraper.Model
{
    public class WorkerUptime(IWorker worker, double uptimePercentage) : IWorkerUptime
    {
        public string Algorithm { get; } = worker.Algorithm;

        public IWorkerId WorkerId { get; } = worker.WorkerId;

        public string Name { get; } = worker.Name;

        public double UptimePercentage { get; } = uptimePercentage;

        public WorkerModel Model { get; } = worker.Model;

        public Farm FarmId { get; } = worker.FarmId;
    }
}
