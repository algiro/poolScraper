using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class WorkerModelDTO(int id, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
    }

}