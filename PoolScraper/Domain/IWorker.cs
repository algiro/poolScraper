using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using PoolScraper.Model;

namespace PoolScraper.Domain
{
    public interface IWorker : IComparable
    {
        IWorkerId WorkerId { get; }
        string Algorithm { get; }
        string Name { get; }
        WorkerModel Model { get; }
        Farm FarmId { get; }
    }

    public static class Worker
    {
        public static IWorker Create(string poolId, string algorithm, long id, string name)
        {
            WorkerModelExtensions.TryGetModel(name, out var workerModel);
            FarmExtension.TryGetFarm(name, out var farm);
            return Create(poolId, algorithm, id, name, workerModel, farm);
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
            return new DefaultWorker(poolId, algorithm, id, name, model, farm);
        }
        public static IEnumerable<WorkerReadModel> AsWorkersReadModel(this IEnumerable<IWorker> workers)
        {
            return workers.Select(w => new WorkerReadModel(w.WorkerId.PoolId,w.Algorithm,w.WorkerId.Id, w.Name,w.Model,w.FarmId));
        }

        private class DefaultWorker : IWorker
        {
            public DefaultWorker(string poolId, string algorithm, long id, string name, WorkerModel model, Farm farm)
            {
                WorkerId = PoolScraper.Domain.WorkerId.Create(poolId, id);
                Algorithm = algorithm;
                Id = id;
                Name = name;
                Model = model;
                FarmId = farm;
            }
            public IWorkerId WorkerId { get; }
            public string Algorithm { get; }
            public long Id { get; }
            public string Name { get; }
            public WorkerModel Model { get; }
            public Farm FarmId { get; }
            public int CompareTo(object? obj)
            {
                if (obj is IWorker other)
                    return WorkerId.CompareTo(other.WorkerId);
                return 0;
            }
        }
    }
}