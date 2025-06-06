﻿using PoolScraper.Domain;

namespace PoolScraper.View
{
    public class WorkerDTO 
    {
        public string Algorithm { get; set; } = string.Empty;
        public WorkerIdDTO WorkerId { get; set; } = Domain.WorkerId.UNINITIALIZED.AsDTO();
        public string Name { get; set; } = string.Empty;
        public long NominalHashRate { get; set; }
        public string Provider { get; set;} = string.Empty;
        public bool IsDisabled { get; set; }
        public WorkerModelDTO Model { get; set; } = new WorkerModelDTO(0, string.Empty);
        public FarmDTO Farm { get; set; } = new FarmDTO(0, string.Empty, string.Empty);
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
                WorkerId = worker.WorkerId.AsDTO(),
                Name = worker.Name,
                NominalHashRate = worker.NominalHashRate,
                Model = worker.Model.AsModelDTO(),
                Farm = worker.Farm.AsFarmDTO(),
                IsDisabled = isDisabled
            };
        }
    }
}