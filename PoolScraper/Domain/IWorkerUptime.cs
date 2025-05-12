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

            public IWorkerModel Model { get; } = worker.Model;

            public IFarm Farm { get; } = worker.Farm;

            public long NominalHashRate { get; } = worker.NominalHashRate;

            public int CompareTo(object? obj)
            {
                if (obj is IWorkerUptime other)
                {                 
                    return WorkerId.CompareTo(other.WorkerId);
                }
                return 0;
            }
        }
    }
}