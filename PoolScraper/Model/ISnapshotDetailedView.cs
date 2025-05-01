using CommonUtils.Utils;
using PoolScraper.Model.Consolidation;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service.Store;

namespace PoolScraper.Model
{
    public interface ISnapshotDetailedView
    {
        string Id { get; }
        Granularity Granularity { get; }
        IWorker Worker { get; }
        IDateRange DateRange { get; }
        IWorkerBasicInfo BasicInfo { get; }
    }

    public static class SnapshotDetailedView
    {
        public static ISnapshotDetailedView? AsSnapshotDetailedView(this ISnapshotWorkerStatus snapshotWorkerStatus, IWorker worker)
        {
            Console.WriteLine("AsSnapshotDetailedView called identified worker:" + worker);
            return new SnapshotDetailedViewImpl(snapshotWorkerStatus, worker);
        }
        public static ISnapshotDetailedView? AsSnapshotDetailedView(this ISnapshotWorkerStatus snapshotWorkerStatus, IWorkerStore workerStore)
        {
            Console.WriteLine("AsSnapshotDetailedView called for workerId:" + snapshotWorkerStatus.WorkerId);
            var worker = workerStore.GetById(snapshotWorkerStatus.WorkerId.Id);
            return worker != null ? snapshotWorkerStatus.AsSnapshotDetailedView(worker) : null;
        }
        private class SnapshotDetailedViewImpl : ISnapshotDetailedView
        {
            public string Id { get; }
            public Granularity Granularity { get; }
            public IWorker Worker { get; }
            public IDateRange DateRange { get; }
            public IWorkerBasicInfo BasicInfo { get; }
            public SnapshotDetailedViewImpl(ISnapshotWorkerStatus snapshotWorkerStatus, IWorker worker)
            {
                Worker = worker;
                Id = snapshotWorkerStatus.Id;
                Granularity = snapshotWorkerStatus.Granularity;
                DateRange = snapshotWorkerStatus.DateRange;
                BasicInfo = snapshotWorkerStatus.BasicInfo;
            }
        }
    }
}
