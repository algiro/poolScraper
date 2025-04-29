namespace PoolScraper.Model
{
    public interface IUptimePeriod
    {
        DateTime From { get; }
        DateTime To { get; }
        bool IsActive { get; }
    }
}
