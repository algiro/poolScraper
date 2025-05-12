using CommonUtils.Utils;
using CommonUtils.Utils.Logs;
using Newtonsoft.Json;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

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

        public IExternalId GetExternalId(IPool pool) => ExternalId.Create(pool.PoolId, Id);
    }

    public static class WorkerStatusExtension
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger(nameof(WorkerStatusExtension));
        public static ISnapshotWorkerStatus AsWorkerMinuteStatus(this WorkerStatus workerStatus, IPool pool, IDateRange dateRange)
        {
            var externalId = workerStatus.GetExternalId(pool);
            var workerId = WorkerIdMap.Instance.GetWorkerId(externalId);
            return SnapshotWorkerStatus.Create(workerId, Granularity.Minutes, dateRange, WorkerBasicInfo.Create(workerStatus.Hashrate, workerStatus.InvalidShares));
        }
        public static IEnumerable<ISnapshotWorkerStatus> AsWorkersMinuteStatus(this IEnumerable<WorkerStatus> workersStatus, IPool pool, IDateRange dateRange)
        {
            var result = workersStatus.SelectNotNull(w => w.AsWorkerMinuteStatus(pool, dateRange));
            
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
