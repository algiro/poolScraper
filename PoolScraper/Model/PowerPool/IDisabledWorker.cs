using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public interface IDisabledWorker
    {
        string PoolId { get; }
        long Id { get; }
    }  
}
