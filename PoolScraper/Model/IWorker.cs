using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace PoolScraper.Model
{
    public interface IWorker
    {
        long Id { get; }
        string Algorithm { get; }
        string Name { get; }
        string PoolId { get; }
        WorkerModel Model { get; }
        Farm FarmId { get; }
    }

    public static class WorkerExtensions
    {
        public static IWorker Create(string poolId, string algorithm, long id, string name)
        {
            WorkerModelExtensions.TryGetModel(name, out var workerModel);
            FarmExtension.TryGetFarm(name, out var farm);
            return new Worker(poolId, algorithm, id, name, workerModel, farm);
        }
        public static string GetWorkerSuffix(string workerName)
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
            return new Worker(poolId, algorithm, id, name, model, farm);
        }
        public static IEnumerable<Worker> AsWorkers(this IEnumerable<IWorker> workers)
        {
            return workers.Select(w => new Worker(w.PoolId,w.Algorithm,w.Id,w.Name,w.Model,w.FarmId));
        }
    }
}