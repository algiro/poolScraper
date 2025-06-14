using PoolScraper.Domain;

namespace PoolScraper.Service
{
    public interface IAdminService
    {
        Task<bool> RestoreCollectionsFromScraping(IDateRange dataRange);
    }
}
