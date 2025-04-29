using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Model
{
    public class WorkerIdView(string poolId, long id) : IWorkerId
    {
        public string PoolId { get; set; } = poolId;
        public long Id { get; set; } = id;
    }

    public static class WorkerIdViewExtension
    {
        public static IWorkerId AsWorkerId(this WorkerIdView workerIdView)
        {
            return WorkerId.Create(workerIdView.PoolId, workerIdView.Id);
        }
    }
}