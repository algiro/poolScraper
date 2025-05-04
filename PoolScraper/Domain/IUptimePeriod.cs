namespace PoolScraper.Domain
{
    public interface IUptimePeriod
    {
        DateTime From { get; }
        DateTime To { get; }
        bool IsActive { get; }
    }
    public class UptimePeriod
    {
        public static IUptimePeriod Create(DateTime from, DateTime to, bool periodState)
            => new DefaultUptimePeriod(from, to, periodState);

        private readonly struct DefaultUptimePeriod(DateTime from, DateTime to, bool isActive) : IUptimePeriod
        {
            public DateTime From { get; } = from;
            public DateTime To { get; } = to;
            public bool IsActive { get; } = isActive;
        }
    }
}
