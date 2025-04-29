using PoolScraper.Model;
using PoolScraper.Model.Consolidation;

namespace PoolScraper.Persistency.Consolidation
{
    public interface IUptimeHourConsolidationPersistency
    {
        Task<IEnumerable<IUptimePercentage>> GetHourlyUptimeAsync(DateOnly dateOnly);
        Task<bool> InsertManyAsync(int hour, IEnumerable<IUptimePercentage> hourlyUptime);
    }
}