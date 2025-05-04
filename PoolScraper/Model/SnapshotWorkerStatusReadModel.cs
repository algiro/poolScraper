using MongoDB.Bson.Serialization.Attributes;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Model
{
    public class SnapshotWorkerStatusReadModel(ISnapshotWorkerStatus snapshotWorkerStatus)
    {
        [BsonId]
        public string Id { get; } = snapshotWorkerStatus.Id;
        public Granularity Granularity { get; set; } = snapshotWorkerStatus.Granularity;
        public WorkerIdReadModel WorkerId { get; set; } = snapshotWorkerStatus.WorkerId.AsWorkerIdView();
        public DateRangeReadModel DateRange { get; set; } = snapshotWorkerStatus.DateRange.AsDateRangeView();
        public WorkerBasicInfoReadModel BasicInfo { get; set; } = snapshotWorkerStatus.BasicInfo.AsWorkerBasicInfoView();
    }

    public static class SnapshotWorkerStatusReadModelExtension
    {
        public static ISnapshotWorkerStatus AsSnapshotWorkerStatus(this SnapshotWorkerStatusReadModel snapshotWorkerStatusView)
        {
            return SnapshotWorkerStatus.Create(
                snapshotWorkerStatusView.WorkerId.AsWorkerId(),
                snapshotWorkerStatusView.Granularity,
                snapshotWorkerStatusView.DateRange.AsDateRange(),
                snapshotWorkerStatusView.BasicInfo.AsWorkerBasicInfo());
        }
        public static SnapshotWorkerStatusReadModel AsSnapshotWorkerStatusView(this ISnapshotWorkerStatus snapshotWorkerStatus) => new SnapshotWorkerStatusReadModel(snapshotWorkerStatus);
    }
}