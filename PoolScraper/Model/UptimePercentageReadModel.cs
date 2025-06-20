﻿using MongoDB.Bson.Serialization.Attributes;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Service.Uptime;

namespace PoolScraper.Model
{
    public class UptimePercentageReadModel(Granularity granularity, IWorkerId workerId, IDateRange dateRange, double uptimePercentage)
    {
        [BsonId]
        public string Id => $"{workerId.PoolId}.{workerId.Id}.{Granularity.GetId(dateRange.From)}";
        public WorkerIdReadModel WorkerId { get; set; } = workerId.AsReadModel();
        public DateRangeReadModel DateRange { get; set; } = dateRange.AsDateRangeView();
        public Granularity Granularity { get; set; } = granularity;
        public double UptimePercentage { get; set; } = uptimePercentage;
    }

    public static class UptimePercentageReadModelExtension
    {
        public static IUptimePercentage AsUptimePercentage(this UptimePercentageReadModel uptimePercentageView)
        {
            return UptimePercentage.Create(uptimePercentageView.WorkerId.AsWorkerId(), uptimePercentageView.DateRange, uptimePercentageView.UptimePercentage);
        }
    }
}
