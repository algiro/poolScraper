using PoolScraper.Domain.Consolidation;
using PoolScraper.Model;
using PoolScraper.Service.Store;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IUptimePercentageDetailedView:IUptimePercentage
    {
        IWorker Worker { get; }
    }
    public static class UptimePercentageDetailedView
    {
        public static IUptimePercentageDetailedView? AsUptimePercentageDetailedView(this IUptimePercentage uptimePercentage, IWorker worker)
        {
            return new UptimePercentageDetailedViewImpl(uptimePercentage, worker);
        }
        public static IUptimePercentageDetailedView? AsUptimePercentageDetailedView(this IUptimePercentage uptimePercentage, IWorkerStore workerStore)
        {
            var result = workerStore.GetById(uptimePercentage.WorkerId);
            return result.worker != null && !result.isDisabled ? uptimePercentage.AsUptimePercentageDetailedView(result.worker) : null;
        }
        private class UptimePercentageDetailedViewImpl(IUptimePercentage uptimePercentage, IWorker worker) : IUptimePercentageDetailedView
        {
            public IWorker Worker { get; } = worker;
            public IWorkerId WorkerId { get; } = worker.WorkerId;
            public IDateRange DateRange { get; } = uptimePercentage.DateRange;

            public double UptimePercentage { get; } = uptimePercentage.UptimePercentage;
        }
    }
}
