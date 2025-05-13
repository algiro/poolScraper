using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IWorkerIdMap
    {
        bool TryGetWorkerId(IExternalId externalId, [NotNullWhen(true)] out IWorkerId? workerId);
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
            public bool TryGetWorkerId(IExternalId externalId, [NotNullWhen(true)] out IWorkerId? workerId)
                => workerIdMap.TryGetValue(externalId, out workerId);
        }
    }

}
