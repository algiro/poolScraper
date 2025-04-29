using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public interface IWorker
    {
        [JsonProperty("algorithm")]
        string Algorithm { get; }
        [JsonProperty("id")]
        long Id { get; }
        [JsonProperty("name")]
        string Name { get; }
        [JsonProperty("poolId")]
        string PoolId { get; }
    }

    public static class WorkerExtensions
    {
        public static IEnumerable<Worker> AsWorkers(this IEnumerable<IWorker> workers)
        {
            return workers.Select(w => new Worker(w.PoolId,w.Algorithm,w.Id,w.Name));
        }
    }
}