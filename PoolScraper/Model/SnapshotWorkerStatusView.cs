using MongoDB.Bson.Serialization.Attributes;
using PoolScraper.Model.Consolidation;

namespace PoolScraper.Model
{
    public class SnapshotWorkerStatusView(ISnapshotWorkerStatus snapshotWorkerStatus)
    {
        [BsonId]
        public string Id { get; } = snapshotWorkerStatus.Id;
        public Granularity Granularity { get; set; } = snapshotWorkerStatus.Granularity;
        public WorkerIdView WorkerId { get; set; } = snapshotWorkerStatus.WorkerId.AsWorkerIdView();
        public DateRangeView DateRange { get; set; } = snapshotWorkerStatus.DateRange.AsDateRangeView();
        public WorkerBasicInfoView BasicInfo { get; set; } = snapshotWorkerStatus.BasicInfo.AsWorkerBasicInfoView();
    }

    public static class SnapshotWorkerStatusViewExtension
    {
        public static ISnapshotWorkerStatus AsSnapshotWorkerStatus(this SnapshotWorkerStatusView snapshotWorkerStatusView)
        {
            return SnapshotWorkerStatus.Create(
                snapshotWorkerStatusView.WorkerId.AsWorkerId(),
                snapshotWorkerStatusView.Granularity,
                snapshotWorkerStatusView.DateRange.AsDateRange(),
                snapshotWorkerStatusView.BasicInfo.AsWorkerBasicInfo());
        }
        public static SnapshotWorkerStatusView AsSnapshotWorkerStatusView(this ISnapshotWorkerStatus snapshotWorkerStatus) => new SnapshotWorkerStatusView(snapshotWorkerStatus);
    }
}