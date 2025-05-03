using PoolScraper.Model;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.View
{
    public class WorkerDTO : IWorker
    {
        public string Algorithm { get; set; } = string.Empty;
        public IWorkerId WorkerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public WorkerModel Model { get; set; }
        public Farm FarmId { get; set; }
    }

    public static class WorkerDTOExtensions
    {
        public static WorkerDTO ToWorkerDTO(this IWorker worker, IEnumerable<IDisabledWorker> disabledWorkers)
            => ToWorkerDTO(worker, disabledWorkers.Any(dw => dw.PoolId == worker.WorkerId.PoolId && dw.Id == worker.WorkerId.Id));
        public static WorkerDTO ToWorkerDTO(this IWorker worker, bool isDisabled)
        {
            return new WorkerDTO
            {
                Algorithm = worker.Algorithm,
                WorkerId = worker.WorkerId,
                Name = worker.Name,
                Model = worker.Model,
                FarmId = worker.FarmId,
                IsDisabled = isDisabled
            };
        }
    }
}