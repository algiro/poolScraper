
using log4net;
using MongoDB.Driver;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model;
using PoolScraper.Service.Uptime;
using PoolScraper.Persistency;
using PoolScraper.Domain;
using PoolScraper.Service.Store;
using CommonUtils.Utils;

namespace PoolScraper.Service
{
    public class UptimeService(
        ILogger<UptimeService> logger, 
        IPowerPoolScrapingService powerPoolScrapingService,
        IWorkerPersistency workerPersistency,
        IWorkerStore workerStore) : IUptimeService
    {
        private readonly IPool powerPool = Pool.CreatePowerPool();

        /// <summary>
        /// Calculates uptime statistics for each worker (identified by WorkerStatus.Id) over the specified time range.
        /// Uptime is computed as the percentage of minute snapshots where the worker was present.
        /// </summary>
        /// <param name="apiKey">The API key used to filter the documents.</param>
        /// <param name="from">Start of the time range.</param>
        /// <param name="to">End of the time range.</param>
        /// <returns>
        /// A dictionary mapping each WorkerStatus.Id (long) to its uptime percentage (double).
        /// For example: { 4602404 : 100.0, 4602380 : 90.0, ... }
        /// </returns>
        public async Task<IEnumerable<IWorkerUptime>> GetWorkerUptimeStatsAsync(DateTime from, DateTime to)
        {
            return await GetUptimeNewImplementation(from, to);
        }
        
        private async Task<IEnumerable<IWorkerUptime>> GetUptimeNewImplementation(DateTime from, DateTime to)
        {
            logger.LogInformation("GetUptimeNewImplementation for dateRange: {dateFrom} - {dateTo}", from, to);

            var documents = await powerPoolScrapingService.GetDataRangeAsync(from, to);
            int totalSnapshots = documents.Count();
            logger.LogInformation("Total snapshots collected: {totalSnapshots}", totalSnapshots);
            // If no snapshots collected, return an empty dictionary.
            if (totalSnapshots == 0)
                return Enumerable.Empty<IWorkerUptime>();
            UptimeCalculator uptimeCalculator = new UptimeCalculator();
            var snapshotWorkerStatus = documents.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap());
            var allWorkers = await workerPersistency.GetAllWorkerAsync();
            var workerUptimeResult = uptimeCalculator.CalculateTotUptime(snapshotWorkerStatus);
            return workerUptimeResult.SelectNotNull(w =>
                {
                    var worker = allWorkers.FirstOrDefault(wk => wk.WorkerId.Equals(w.WorkerId));
                    if (worker == null)
                    {
                        return null;
                    }
                    else
                    {
                        return WorkerUptime.Create(worker, w.UptimePercentage);
                    }
                });
        }

        public async Task<IEnumerable<IUptimePeriod>> GetWorkerUptimeHistoryAsync(string poolId, long workerId, DateTime from, DateTime to)
        {
            var documents = await powerPoolScrapingService.GetDataRangeAsync(from, to);
            /*   IEnumerable<(DateTime fetchedAt,IEnumerable<AlgorithmWorkers> algo)> miners = documents.Select(d => (d.FetchedAt, d.GetAllAlgoWorkers()));
               IEnumerable<(DateTime fetchedAt,WorkerStatus workerStatus)> workers = miners.Select(m => (m.fetchedAt, m.algo.SelectMany(w => w.GetAllWorkerStatus()).Single(w => w.Id == workerId)));
               var history =  workers.Select(w => WorkerUptimeHistory.Create(w.fetchedAt, w.workerStatus.Hashrate > 0));*/
            var snapshotWorkerStatus = documents.AsSnapshotWorkerStatus(workerStore.GetWorkerIdMap()).Where(s => s.WorkerId.Equals(WorkerId.Create(poolId,workerId)));
            logger.LogInformation("GetWorkerUptimeHistoryAsync for workerId: {workerId} with {count}# snapshots", workerId, snapshotWorkerStatus.Count());
            var history = snapshotWorkerStatus.Select(s => WorkerUptimeHistory.Create(s.DateRange.From, s.BasicInfo.Hashrate > 0));
            return UptimePeriods.CreatePeriods(history);
        }
    }
}
