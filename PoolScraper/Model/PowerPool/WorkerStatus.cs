using CommonUtils.Utils;
using CommonUtils.Utils.Logs;
using Newtonsoft.Json;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Service.Store;

namespace PoolScraper.Model.PowerPool
{
    public class WorkerStatus
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("algorithm")]
        public string Algorithm { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("valid_shares")]
        public double ValidShares { get; set; }

        [JsonProperty("invalid_shares")]
        public double InvalidShares { get; set; }

        [JsonProperty("stale_shares")]
        public double StaleShares { get; set; }

        [JsonProperty("blocks")]
        public int Blocks { get; set; }

        [JsonProperty("hashrate")]
        public double Hashrate { get; set; }

        [JsonProperty("hashrate_units")]
        public string HashrateUnits { get; set; } = string.Empty;

        [JsonProperty("hashrate_avg")]
        public double HashrateAvg { get; set; }

        [JsonProperty("hashrate_avg_units")]
        public string HashrateAvgUnits { get; set; } = string.Empty;

        public IExternalId GetExternalId(IPool pool) => ExternalId.Create(pool.PoolId, Id.ToString());
    }

    public static class WorkerStatusExtension
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger(nameof(WorkerStatusExtension));
        public static INewWorker AsNewWorker(this WorkerStatus workerStatus, IPool pool) 
        {
            var worker = Worker.CreateNew(pool.PoolId, workerStatus.Algorithm, workerStatus.Name);
            return NewWorker.CreateNew(worker, workerStatus.GetExternalId(pool));
        }
        public static ISnapshotWorkerStatus? AsWorkerMinuteStatus(this WorkerStatus workerStatus, IWorkerIdMap workedIdMap, IPool pool, IDateRange dateRange)
        {
            var externalId = workerStatus.GetExternalId(pool);
            if (workedIdMap.TryGetWorkerId(externalId, out var workerId)) { 
                return SnapshotWorkerStatus.Create(workerId, Granularity.Minutes, dateRange, WorkerBasicInfo.Create(workerStatus.Hashrate, workerStatus.InvalidShares));
            }
            else
            {
                logger.LogWarning($"WorkerId not found for externalId: {externalId}");
                return null;
            }
        }
        public static (IEnumerable<IWorkerId> Matching,IEnumerable<IExternalId> Added, IEnumerable<IWorkerId> Removed) GetWorkerIdsStatus(this IEnumerable<WorkerStatus> workersStatus, IWorkerIdMap workerIdMap, IPool pool)
        {
            logger.LogInformation("GetWorkerIdsStatus called with {count}# workersStatus, matching with workerIdMap#: {workerIdMap}", workersStatus.Count(), workerIdMap.GetWorkerIds().Count());
            List<IWorkerId> matching = new List<IWorkerId>();
            List<IExternalId> added  = new List<IExternalId>();
            List<IWorkerId> removed  = new List<IWorkerId>();
            foreach (var workerStatus in workersStatus)
            {
                var externalId = workerStatus.GetExternalId(pool);
                if (workerIdMap.TryGetWorkerId(externalId, out var workerId))
                {
                    matching.Add(workerId);
                }
                else
                {
                    added.Add(externalId);
                }
            }
            var existingExternalIds = workerIdMap.GetExternalIds();
            var currentExternalIds = workersStatus.Select(w => w.GetExternalId(pool));
            var removedExternalIds = existingExternalIds.Except(currentExternalIds);
            var removedWorkerIds  = removedExternalIds.SelectNotNull(r => {
                if (workerIdMap.TryGetWorkerId(r, out var workerId)) return workerId;
                else return null;
                }).ToList();

            return (matching, added, removedWorkerIds);
        }

        public static IEnumerable<ISnapshotWorkerStatus> AsWorkersMinuteStatus(this IEnumerable<WorkerStatus> workersStatus, IWorkerIdMap workedIdMap, IPool pool, IDateRange dateRange)
        {
            var result = workersStatus.SelectNotNull(w => w.AsWorkerMinuteStatus(workedIdMap, pool, dateRange));
            
            // verify if we have same WorkerId in the same dateRange
            var duplicates = result.GroupBy(w => w.WorkerId).Where(g => g.Count() > 1);
            if (duplicates.Count() > 0)
            {
                throw new ArgumentException("The snapshot contains multiple entries for the same workerId in the same date range.");
            }
            return result;
        }
    }
}
