using PoolScraper.Domain.Consolidation;
using PoolScraper.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace PoolScraper.Model
{
    public class SnapshotDataConsolidationInfoReadModel(ISnapshotDataConsolidationInfo snapshotDataConsolidation)
    {
        [BsonId]
        public string Id => $"{Granularity}.{snapshotDataConsolidation.Granularity.GetId(snapshotDataConsolidation.DateRange)}";
        public string Granularity { get; set; } = snapshotDataConsolidation.Granularity.ToString();
        public DateRangeReadModel DateRange { get; set; } = snapshotDataConsolidation.DateRange.AsDateRangeView();
    }

    public static class SnapshotDataConsolidationInfoViewExtension
    {
        public static ISnapshotDataConsolidationInfo AsSnapshotDataConsolidationInfo(this SnapshotDataConsolidationInfoReadModel snapshotDataConsolidationInfoView)
        {
            return SnapshotDataConsolidationInfo.Create(Enum.Parse<Granularity>(snapshotDataConsolidationInfoView.Granularity), snapshotDataConsolidationInfoView.DateRange.AsDateRange());
        }
        
        public static SnapshotDataConsolidationInfoReadModel AsSnapshotDataConsolidationInfoReadModel(this ISnapshotDataConsolidationInfo snapshotDataConsolidationInfo) 
            => new SnapshotDataConsolidationInfoReadModel(snapshotDataConsolidationInfo);
    }
}
