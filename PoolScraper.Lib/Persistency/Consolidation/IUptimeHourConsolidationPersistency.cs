using PoolScraper.Domain;

namespace PoolScraper.Persistency.Consolidation
{
    public interface IUptimeHourConsolidationPersistency
    {
        Task<IEnumerable<IUptimePercentage>> GetHourlyUptimeAsync(DateOnly dateOnly);
        Task<bool> InsertManyAsync(int hour, IEnumerable<IUptimePercentage> hourlyUptime);
    }
}