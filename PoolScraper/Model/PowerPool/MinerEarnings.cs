using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public class MinerEarnings
    {
        // Changed properties to lists since the JSON returns arrays.
        [JsonProperty("scrypt")]
        public List<AlgorithmEarnings> Scrypt { get; set; } = new List<AlgorithmEarnings>();
        [JsonProperty("sha256")]
        public List<AlgorithmEarnings> Sha256 { get; set; } = new List<AlgorithmEarnings>();
        // Add additional algorithms if needed.
    }
}
