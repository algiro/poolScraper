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
    }

    public static class WorkerStatusExtension
    {
        public static ISnapshotWorkerStatus AsWorkerMinuteStatus(this WorkerStatus workerStatus, IPool pool, IDateRange dateRange)
            => SnapshotWorkerStatus.Create(WorkerId.Create(pool.PoolId, workerStatus.Id), Granularity.Minutes, dateRange, WorkerBasicInfo.Create(workerStatus.Hashrate, workerStatus.InvalidShares));
        public static IEnumerable<ISnapshotWorkerStatus> AsWorkersMinuteStatus(this IEnumerable<WorkerStatus> workersStatus, IPool pool, IDateRange dateRange)
            => workersStatus.Select(w => w.AsWorkerMinuteStatus(pool, dateRange));
    }
}
