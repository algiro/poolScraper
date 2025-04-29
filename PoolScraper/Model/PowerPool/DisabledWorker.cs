using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public class DisabledWorker : IDisabledWorker
    {
        public DisabledWorker(string poolId, long id)
        {
            PoolId = poolId;
            Id = id;
        }
        [JsonProperty("poolId")]
        public string PoolId { get; set; }
        [JsonProperty("id")] 
        public long Id { get; set; }

        public override int GetHashCode()
        {
            return PoolId.GetHashCode() ^ Id.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is IWorker other)
            {
                return PoolId == other.PoolId &&
                       Id == other.Id;
            }
            return false;
        }
    }
}
