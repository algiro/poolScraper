using PoolScraper.Domain.Consolidation;
using PoolScraper.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace PoolScraper.Model
{
    public class UptimeDataConsolidationInfoReadModel(IUptimeDataConsolidationInfo uptimeDataConsInfo)
    {
        [BsonId]
        public string Id => $"{Granularity}.{uptimeDataConsInfo.Granularity.GetId(uptimeDataConsInfo.DateRange)}";
        public string Granularity { get; set; } = uptimeDataConsInfo.Granularity.ToString();
        public DateRangeReadModel DateRange { get; set; } = uptimeDataConsInfo.DateRange.AsDateRangeView();
        public int Count { get; set; } = uptimeDataConsInfo.Count;
    }

    public static class UptimeDataConsolidationInfoReadModelExtension
    {
        public static IUptimeDataConsolidationInfo AsUptimeDataConsolidationInfo(this UptimeDataConsolidationInfoReadModel uptimeDataConsolidationRM)
        {
            return UptimeDataConsolidationInfo.Create(Enum.Parse<Granularity>(uptimeDataConsolidationRM.Granularity), uptimeDataConsolidationRM.DateRange.AsDateRange(), uptimeDataConsolidationRM.Count);
        }
        
        public static UptimeDataConsolidationInfoReadModel AsUptimeDataConsolidationInfoReadModel(this IUptimeDataConsolidationInfo uptimeDataConsolidationInfo) 
            => new UptimeDataConsolidationInfoReadModel(uptimeDataConsolidationInfo);
    }
}
