using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class FarmDTO(int id, string name, string location)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Location { get; set; } = location;
    }

    public static class FarmDTOExtensions
    {
        public static IFarm AsFarm(this FarmDTO farmDTO)
        {
            if (Farm.TryGet(farmDTO.Id, out var farm)) { 
                return farm;
            }
            return null;
        }
    }
}