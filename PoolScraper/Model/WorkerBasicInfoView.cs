namespace PoolScraper.Model
{
    public class WorkerBasicInfoView
    {
        public double Hashrate { get; set; }
        public double RejectedShares { get; set; }
    }

    public static class WorkerBasicInfoViewExtension
    {
        public static WorkerBasicInfoView AsWorkerBasicInfoView(this IWorkerBasicInfo workerBasicInfo)
        {
            return new WorkerBasicInfoView
            {
                Hashrate = workerBasicInfo.Hashrate,
                RejectedShares = workerBasicInfo.RejectedShares
            };
        }
        public static IWorkerBasicInfo AsWorkerBasicInfo(this WorkerBasicInfoView workerBasicInfoView)
        {
            return WorkerBasicInfo.Create(workerBasicInfoView.Hashrate, workerBasicInfoView.RejectedShares);
        }
    }
}
