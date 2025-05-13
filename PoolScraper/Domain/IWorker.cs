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
        IWorkerModel Model { get; }
        IFarm Farm { get; }
        long NominalHashRate { get; }
    }

    public static class Worker
    {
        public static IWorker CreateNew(string poolId, string algorithm, string name) => Create(poolId, algorithm, -1, name);

        public static IWorker Create(string poolId, string algorithm, long id, string name)
        {
            WorkerModelExtensions.TryGetModel(name, out var workerModel);
            Farm.TryGetFarm(name, out var farm);
            WorkerModelExtensions.TryGetNominalHashRate(name, out long nominalHashRate);
            return Create(poolId, algorithm, id, name, nominalHashRate, workerModel, farm);
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

        public static IWorker Create(string poolId, string algorithm, long id, string name, long nominalHashRate, IWorkerModel model, IFarm farm)
        {            
            return new DefaultWorker(poolId, algorithm, id, name,nominalHashRate, model, farm);
        }
        public static IEnumerable<WorkerReadModel> AsWorkersReadModel(this IEnumerable<IWorker> workers)
        {
            return workers.Select(w => new WorkerReadModel(w.WorkerId.PoolId,w.Algorithm,w.WorkerId.Id, w.Name,w.NominalHashRate, w.Model,w.Farm));
        }

        private class DefaultWorker : IWorker
        {
            public DefaultWorker(string poolId, string algorithm, long id, string name, long nominalHashRate, IWorkerModel model, IFarm farm)
            {
                WorkerId = PoolScraper.Domain.WorkerId.Create(poolId, id);
                Algorithm = algorithm;
                Id = id;
                NominalHashRate = nominalHashRate;
                Name = name;
                Model = model;
                Farm = farm;
            }
            public IWorkerId WorkerId { get; }
            public string Algorithm { get; }
            public long Id { get; }
            public string Name { get; }
            public IWorkerModel Model { get; }
            public IFarm Farm { get; }
            public long NominalHashRate { get; }

            public int CompareTo(object? obj)
            {
                if (obj is IWorker other)
                    return WorkerId.CompareTo(other.WorkerId);
                return 0;
            }
        }
    }
}