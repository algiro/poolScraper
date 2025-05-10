using CommonUtils.Utils;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service.Store;

namespace PoolScraper.Domain
{
    public interface ISnapshotDetailedView : ISnapshotWorkerStatus
    {
        IWorker Worker { get; }
    }

    public static class SnapshotDetailedView
    {
        public static ISnapshotDetailedView? AsSnapshotDetailedView(this ISnapshotWorkerStatus snapshotWorkerStatus, IWorker worker)
        {
            return new SnapshotDetailedViewImpl(snapshotWorkerStatus, worker);
        }
        public static ISnapshotDetailedView? AsSnapshotDetailedView(this ISnapshotWorkerStatus snapshotWorkerStatus, IWorkerStore workerStore)
        {
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
            public IWorkerId WorkerId => Worker.WorkerId;

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
