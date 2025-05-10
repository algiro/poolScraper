using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class WorkerIdDTO(string poolId, long id)
    {
        public string PoolId { get; set; } = poolId;
        public long Id { get; set; } = id;
    }

    public static class WorkerIdDTOExtensions
    {
        public static IWorkerId AsWorkerId(this WorkerIdDTO workerIdView)
        {
            return WorkerId.Create(workerIdView.PoolId, workerIdView.Id);
        }
    }
}