using PoolScraper.Domain;

namespace PoolScraper.View
{
    public class WorkerDTO 
    {
        public string Algorithm { get; set; } = string.Empty;
        public WorkerIdDTO WorkerId { get; set; } = Domain.WorkerId.UNINITIALIZED.AsWorkerIdDTO();
        public string Name { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public WorkerModel Model { get; set; }
        public Farm FarmId { get; set; }
    }

    public static class WorkerDTOExtensions
    {
        public static WorkerDTO ToWorkerDTO(this IWorker worker, IEnumerable<IDisabledWorker> disabledWorkers)
            => ToWorkerDTO(worker, disabledWorkers.Any(dw => dw.WorkerId.Equals(worker.WorkerId)));
        public static WorkerDTO ToWorkerDTO(this IWorker worker, bool isDisabled)
        {
            return new WorkerDTO
            {
                Algorithm = worker.Algorithm,
                WorkerId = worker.WorkerId.AsWorkerIdDTO(),
                Name = worker.Name,
                Model = worker.Model,
                FarmId = worker.FarmId,
                IsDisabled = isDisabled
            };
        }
    }
}