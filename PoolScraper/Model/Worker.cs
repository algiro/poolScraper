using Newtonsoft.Json;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.Model
{
    public class Worker : IWorker
    {
        public Worker(string poolId, string algorithm, long id, string name)
        {
            PoolId = poolId;
            Algorithm = algorithm;
            Id = id;
            Name = name;
        }
        [JsonProperty("poolId")]
        public string PoolId { get; set; }
        [JsonProperty("id")] 
        public long Id { get; set; }
        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return PoolId.GetHashCode() ^ Algorithm.GetHashCode() ^ Id.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is IWorker other)
            {
                return PoolId == other.PoolId &&
                       Algorithm == other.Algorithm &&
                       Id == other.Id &&
                       Name == other.Name;
            }
            return false;
        }
    }
}
