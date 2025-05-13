using PoolScraper.Model;
using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IWorkerIdMatch
    {
        IWorkerId WorkerId { get; }
        IExternalId ExternalId { get; }
    }

    public static class WorkerIdMatch
    {
        public static IWorkerIdMatch UNINITIALIZED = new WorkerIdMatchImpl(WorkerId.UNINITIALIZED,ExternalId.UNINITIALIZED);
        public static IWorkerIdMatch Create(IWorkerId workerId, IExternalId externalId)
        {
            return new WorkerIdMatchImpl(workerId, externalId);
        }
        public static IEnumerable<WorkerIdMatchReadModel> AsReadModels(this IEnumerable<IWorkerIdMatch> workerIdMatches) =>
            workerIdMatches.Select(w => w.AsReadModel());
        public static WorkerIdMatchReadModel AsReadModel(this IWorkerIdMatch workerIdMatch)
        {
            return new WorkerIdMatchReadModel(workerIdMatch.ExternalId, workerIdMatch.WorkerId);
        }
        private class WorkerIdMatchImpl(IWorkerId workerId,IExternalId externalId) : IWorkerIdMatch
        {
            public IWorkerId WorkerId { get; } = workerId;

            public IExternalId ExternalId { get; } = externalId;

            public override int GetHashCode()
            {
                return WorkerId.GetHashCode() ^ ExternalId.GetHashCode();
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is IWorkerIdMatch other)
                {
                    return WorkerId.Equals(other.WorkerId) && ExternalId.Equals(other.ExternalId);
                }
                return false;
            }
        }
    }
}