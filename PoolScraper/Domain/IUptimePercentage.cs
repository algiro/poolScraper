using PoolScraper.Domain.Consolidation;
using PoolScraper.Model;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IUptimePercentage
    {
        IWorkerId WorkerId { get; }
        IDateRange DateRange { get; }
        double UptimePercentage { get; }
    }
    public static class UptimePercentage
    {
        public static IUptimePercentage Create(IWorkerId workerId, IDateRange dateRange, double uptimePercentage)
        {
            return new UptimePercentageImpl(workerId, dateRange, uptimePercentage);
        }
        public static UptimePercentageReadModel AsUptimePercentageView(this IUptimePercentage uptimePercentage,Granularity granularity)
        {
            return new UptimePercentageReadModel(granularity, uptimePercentage.WorkerId, uptimePercentage.DateRange, uptimePercentage.UptimePercentage);
        }
        private readonly struct UptimePercentageImpl(IWorkerId workerId, IDateRange dateRange, double uptimePercentage) : IUptimePercentage
        {
            public IWorkerId WorkerId { get; } = workerId;
            public IDateRange DateRange { get; } = dateRange;
            public double UptimePercentage { get; } = uptimePercentage;
            public override int GetHashCode()
            {
                return WorkerId.GetHashCode() ^ DateRange.GetHashCode() ^ UptimePercentage.GetHashCode();
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is UptimePercentageImpl other)
                {
                    return WorkerId.Equals(other.WorkerId) && DateRange.Equals(other.DateRange) && UptimePercentage.Equals(other.UptimePercentage);
                }
                return false;
            }
        }
    }
}
