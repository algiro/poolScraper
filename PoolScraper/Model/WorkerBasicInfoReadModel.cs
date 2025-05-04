using PoolScraper.Domain;

namespace PoolScraper.Model
{
    public class WorkerBasicInfoReadModel
    {
        public double Hashrate { get; set; }
        public double RejectedShares { get; set; }
    }

    public static class WorkerBasicInfoReadModelExtension
    {
        public static WorkerBasicInfoReadModel AsWorkerBasicInfoView(this IWorkerBasicInfo workerBasicInfo)
        {
            return new WorkerBasicInfoReadModel
            {
                Hashrate = workerBasicInfo.Hashrate,
                RejectedShares = workerBasicInfo.RejectedShares
            };
        }
        public static IWorkerBasicInfo AsWorkerBasicInfo(this WorkerBasicInfoReadModel workerBasicInfoView)
        {
            return WorkerBasicInfo.Create(workerBasicInfoView.Hashrate, workerBasicInfoView.RejectedShares);
        }
    }
}
