using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IWorkerIdMap
    {
        IWorkerId GetWorkerId(IExternalId externalId);
    }

    public static class WorkerIdMap
    {
        public static IWorkerIdMap Instance { get; private set;  } = new DefaultWorkerIdMap(new Dictionary<IExternalId, IWorkerId>());
        public static void Initialize(IDictionary<IExternalId, IWorkerId> workerIdMap)
        {
            Instance = new DefaultWorkerIdMap(workerIdMap);
        }
        private class DefaultWorkerIdMap(IDictionary<IExternalId, IWorkerId> workerIdMap) : IWorkerIdMap
        {
            public IWorkerId GetWorkerId(IExternalId externalId)
            { 
                if (!workerIdMap.TryGetValue(externalId, out var workerId))
                {
                    // If the workerId is not found, create a new one (the map works as an exception)
                    workerId = WorkerId.Create(externalId.PoolId, externalId.Id);
                }
                return workerId;
            }
        }
    }

}
