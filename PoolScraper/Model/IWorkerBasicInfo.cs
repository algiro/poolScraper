namespace PoolScraper.Model
{
    public interface IWorkerBasicInfo
    {
        double Hashrate { get; }
        double RejectedShares { get; }
    }

    public static class WorkerBasicInfo
    {
        public static IWorkerBasicInfo Create(double hashrate, double rejectedShares)
        {
            return new WorkerBasicInfoImpl(hashrate, rejectedShares);
        }

        private readonly struct WorkerBasicInfoImpl(double hashrate, double rejectedShares) : IWorkerBasicInfo
        {
            public double Hashrate { get; } = hashrate;
            public double RejectedShares { get; } = rejectedShares;
        }
    }
}
