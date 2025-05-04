namespace PoolScraper.Domain
{
    public interface IWorkerUptime : IWorker
    {
        double UptimePercentage { get; }
    }

    public static class WorkerUptime
    {
        public static IWorkerUptime Create(IWorker worker, double uptimePercentage)
        {
            return new DefaultWorkerUptime(worker, uptimePercentage);
        }

        private class DefaultWorkerUptime(IWorker worker, double uptimePercentage) : IWorkerUptime
        {
            public string Algorithm { get; } = worker.Algorithm;

            public IWorkerId WorkerId { get; } = worker.WorkerId;

            public string Name { get; } = worker.Name;

            public double UptimePercentage { get; } = uptimePercentage;

            public WorkerModel Model { get; } = worker.Model;

            public Farm FarmId { get; } = worker.FarmId;
        }
    }
}