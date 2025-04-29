using MongoDB.Bson.Serialization.Attributes;
using PoolScraper.Model.Consolidation;
using PoolScraper.Service.Uptime;

namespace PoolScraper.Model
{
    public class UptimePercentageView(Granularity granularity, IWorkerId workerId, IDateRange dateRange, double uptimePercentage)
    {
        [BsonId]
        public string Id => $"{workerId.PoolId}.{workerId.Id}.{granularity.GetId(dateRange.From)}";
        public WorkerIdView WorkerId { get; set; } = workerId.AsWorkerIdView();
        public DateRangeView DateRange { get; set; } = dateRange.AsDateRangeView();
        public double UptimePercentage { get; set; } = uptimePercentage;
    }

    public static class UptimePercentageViewExtension
    {
        public static IUptimePercentage AsUptimePercentage(this UptimePercentageView uptimePercentageView)
        {
            return UptimePercentage.Create(uptimePercentageView.WorkerId, uptimePercentageView.DateRange, uptimePercentageView.UptimePercentage);
        }
    }
}
