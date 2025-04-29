using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class WorkerDTO : IWorker
    {
        public string Algorithm { get; set; } = string.Empty;
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PoolId { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
    }

    public static class WorkerDTOExtensions
    {
        public static WorkerDTO ToWorkerDTO(this IWorker worker, IEnumerable<IDisabledWorker> disabledWorkers)
            => ToWorkerDTO(worker, disabledWorkers.Any(dw => dw.PoolId == worker.PoolId && dw.Id == worker.Id));
        public static WorkerDTO ToWorkerDTO(this IWorker worker, bool isDisabled)
        {
            return new WorkerDTO
            {
                Algorithm = worker.Algorithm,
                Id = worker.Id,
                Name = worker.Name,
                PoolId = worker.PoolId,
                IsDisabled = isDisabled
            };
        }
    }
}