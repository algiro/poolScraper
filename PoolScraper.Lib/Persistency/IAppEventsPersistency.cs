using PoolScraper.Domain;

namespace PoolScraper.Persistency
{
    public interface IAppEventsPersistency
    {
        Task<IEnumerable<IAppEvent>> GetEvents(IDateRange dateRange);
        Task InsertAsync(IAppEvent appEvent);
    }
}