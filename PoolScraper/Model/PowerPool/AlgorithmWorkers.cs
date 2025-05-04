using Newtonsoft.Json;

namespace PoolScraper.Model.PowerPool
{
    public class AlgorithmWorkers
    {
        [JsonProperty("scrypt")]
        public List<WorkerStatus> Scrypt { get; set; } = new List<WorkerStatus>();

        [JsonProperty("sha256")]
        public List<WorkerStatus> Sha256 { get; set; } = new List<WorkerStatus>();

        [JsonProperty("x11")]
        public List<WorkerStatus> X11 { get; set; } = new List<WorkerStatus>();

        [JsonProperty("kheavyhash")]
        public List<WorkerStatus> Kheavyhash { get; set; } = new List<WorkerStatus>();

        [JsonProperty("eaglesong")]
        public List<WorkerStatus> Eaglesong { get; set; } = new List<WorkerStatus>();

        [JsonProperty("blake2s")]
        public List<WorkerStatus> Blake2s { get; set; } = new List<WorkerStatus>();
    }

    public static class AlgorithmWorkersExtension
    {
        public static IEnumerable<WorkerStatus> GetAllWorkerStatus(this AlgorithmWorkers workers)
        {
            var allWorkers = new List<WorkerStatus>();
            allWorkers.AddRange(workers.Scrypt);
            allWorkers.AddRange(workers.Sha256);
            allWorkers.AddRange(workers.X11);
            allWorkers.AddRange(workers.Kheavyhash);
            allWorkers.AddRange(workers.Eaglesong);
            allWorkers.AddRange(workers.Blake2s);
            return allWorkers;
        }
        public static int GetTotalWorkersCount(this AlgorithmWorkers workers)
        {
            return workers.GetAllWorkerStatus().Count();
        }
        public static int GetTotalWorkersCount(this IEnumerable<(string algo, int count)> workersAlgoCount)
        {
            return workersAlgoCount.Sum(a => a.count);
        }

        public static IEnumerable<(string algo,int count)> GetTotalWorkersAlgoCount(this AlgorithmWorkers workers)
        {
            var workerAlgoCount = new List<(string algo, int count)>();
            workerAlgoCount.Add(("scrypt", workers.Scrypt.Count));
            workerAlgoCount.Add(("sha256", workers.Sha256.Count));
            workerAlgoCount.Add(("x11", workers.X11.Count));
            workerAlgoCount.Add(("kheavyhash", workers.Kheavyhash.Count));
            workerAlgoCount.Add(("eaglesong", workers.Eaglesong.Count));
            workerAlgoCount.Add(("blake2s", workers.Blake2s.Count));
            return workerAlgoCount;
        }

    }
}
