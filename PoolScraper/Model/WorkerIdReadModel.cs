using PoolScraper.Domain;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Model
{
    public class WorkerIdReadModel(string poolId, long id) : IWorkerId
    {
        public string PoolId { get; set; } = poolId;
        public long Id { get; set; } = id;
    }

    public static class WorkerIdReadModelExtension
    {
        public static IWorkerId AsWorkerId(this WorkerIdReadModel workerIdView)
        {
            return WorkerId.Create(workerIdView.PoolId, workerIdView.Id);
        }
    }
}