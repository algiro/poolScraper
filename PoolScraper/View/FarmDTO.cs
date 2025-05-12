using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class FarmDTO(string farmId, string description)
    {
        public string Id { get; set; } = farmId;
        public string Description { get; set; } = description;
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