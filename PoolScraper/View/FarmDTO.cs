using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class FarmDTO(string farmId, string location)
    {
        public string Id { get; set; } = farmId;
        public string Location { get; set; } = location;
    }

    public static class FarmDTOExtensions
    {
        public static IFarm AsFarm(this FarmDTO farmDTO)
        {
            if (Farm.TryGetFarm(farmDTO.Id, out var farm)) { 
                return farm;
            }
            return null;
        }
    }
}