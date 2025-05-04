using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using PoolScraper.Model;

namespace PoolScraper.Domain
{
    public interface IWorker
    {
        IWorkerId WorkerId { get; }
        string Algorithm { get; }
        string Name { get; }
        WorkerModel Model { get; }
        Farm FarmId { get; }
    }

    public static class WorkerExtensions
    {
        public static IWorker Create(string poolId, string algorithm, long id, string name)
        {
            WorkerModelExtensions.TryGetModel(name, out var workerModel);
            FarmExtension.TryGetFarm(name, out var farm);
            return new WorkerReadModel(poolId, algorithm, id, name, workerModel, farm);
        }
        public static string? GetWorkerSuffix(string workerName)
        {
            if (string.IsNullOrEmpty(workerName))
                return null;

            var idx = workerName.LastIndexOf('.');
            if (idx < 0 || idx == workerName.Length - 1)
                return null;

            return workerName.Substring(idx + 1);
        }

        public static IWorker Create(string poolId, string algorithm, long id, string name, WorkerModel model, Farm farm)
        {            
            return new WorkerReadModel(poolId, algorithm, id, name, model, farm);
        }
        public static IEnumerable<WorkerReadModel> AsWorkers(this IEnumerable<IWorker> workers)
        {
            return workers.Select(w => new WorkerReadModel(w.WorkerId.PoolId,w.Algorithm,w.WorkerId.Id, w.Name,w.Model,w.FarmId));
        }
    }
}