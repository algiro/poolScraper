using CommonUtils.Utils.Logs;
using PoolScraper.Persistency;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IWorkerIdMap
    {
        bool TryGetWorkerId(IExternalId externalId, [NotNullWhen(true)] out IWorkerId? workerId);
        bool AddWorkerId(IExternalId externalId, IWorkerId workerId);
        IEnumerable<IWorkerId> GetWorkerIds();
        IEnumerable<IExternalId> GetExternalIds();


    }

    public static class WorkerIdMap
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger(nameof(WorkerIdMap));
        public static IWorkerIdMap Create() => Create(new Dictionary<IExternalId, IWorkerId>());
        public static IWorkerIdMap Create(IDictionary<IExternalId, IWorkerId> workerIdMap) => new DefaultWorkerIdMap(workerIdMap);
        private class DefaultWorkerIdMap(IDictionary<IExternalId, IWorkerId> workerIdMap) : IWorkerIdMap
        {
            public bool AddWorkerId(IExternalId externalId, IWorkerId workerId)
            {
                if (workerIdMap.ContainsKey(externalId))
                {
                    return false;
                }
                workerIdMap[externalId] = workerId;
                return true;
            }

            public IEnumerable<IExternalId> GetExternalIds() => workerIdMap.Keys;

            public IEnumerable<IWorkerId> GetWorkerIds()
            {
                return workerIdMap.Values;
            }


            public bool TryGetWorkerId(IExternalId externalId, [NotNullWhen(true)] out IWorkerId? workerId)
            {                
                var isFound = workerIdMap.TryGetValue(externalId, out workerId);
                if (!isFound)
                {
                    logger.LogOnce(LogLevel.Warning,$"WorkerId not found for externalId: {externalId}");
                }
                return isFound;
            }
        }
    }
}
