using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using PoolScraper.Model;

namespace PoolScraper.Domain
{
    public interface INewWorker : IWorker
    {
        IExternalId ExternalId { get; }
    }

    public static class NewWorker
    {
        public static INewWorker CreateNew(IWorker worker, IExternalId externalId) => new DefaultNewWorker(worker, externalId);
        public static IEnumerable<IWorkerIdMatch> AsWorkerIdMatches(this IEnumerable<INewWorker> newWorkers) => newWorkers.Select(w => w.AsWorkerIdMatch());
        public static IWorkerIdMatch AsWorkerIdMatch(this INewWorker newWorker)
        {
            return WorkerIdMatch.Create(newWorker.WorkerId, newWorker.ExternalId);
        }
        public static INewWorker UpdateId(this INewWorker sourceWorker, long id)
        {
            var workerUpdatedId = Worker.Create(sourceWorker.WorkerId.PoolId, sourceWorker.Algorithm, id, sourceWorker.Name, sourceWorker.NominalHashRate, sourceWorker.Provider, sourceWorker.Model, sourceWorker.Farm);
            return CreateNew(workerUpdatedId, sourceWorker.ExternalId);
        }
            

        private class DefaultNewWorker(IWorker worker, IExternalId externalId) : INewWorker
        {
            public IExternalId ExternalId { get; } = externalId;
            public IWorkerId WorkerId { get; } = worker.WorkerId;
            public string Algorithm { get; } = worker.Algorithm;
            public string Name { get; } = worker.Name;
            public string Provider { get; } = worker.Provider;
            public IWorkerModel Model { get; } = worker.Model;
            public IFarm Farm { get; } = worker.Farm;
            public long NominalHashRate { get; } = worker.NominalHashRate;
            public override int GetHashCode() => worker.GetHashCode();
            public override bool Equals(object? obj) => worker.Equals(obj);
            public int CompareTo(object? obj) => worker.CompareTo(obj);

        }
    }
}