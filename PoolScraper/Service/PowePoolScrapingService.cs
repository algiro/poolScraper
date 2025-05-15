using CommonUtils.Utils;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service.Store;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PoolScraper.Service
{
    public class PowePoolScrapingService : IPowerPoolScrapingService
    {
        

        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "https://api.powerpool.io/api/user";
        private readonly ILogger _log;
        private readonly OnceFlag _areWorkersLoaded = new OnceFlag();

        private readonly IPool powerPool = Pool.CreatePowerPool();
        private readonly IPowerPoolScrapingPersistency _powerPoolScrapingPersistency;
        private readonly IWorkerPersistency _workerPersistency;
        private readonly IWorkerStore _workerStore;

        public PowePoolScrapingService(ILogger<PowePoolScrapingService> log, IPowerPoolScrapingPersistency powerPoolScrapingPersistency, IWorkerPersistency workerPersistency, IWorkerStore workerStore)
        {
            _log = log;
            _httpClient = new HttpClient();
            _powerPoolScrapingPersistency = powerPoolScrapingPersistency;
            _workerPersistency = workerPersistency;
            _workerStore = workerStore;
        }
        private string apiKey => powerPool.ApiKey;
        public async Task FetchAndStoreUserData()
        {
            try
            {
                var allWorkers = _workerStore.GetAllWorker();

                // Fetch data from API
                var response = await _httpClient.GetAsync($"{API_BASE_URL}?apiKey={apiKey}");
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var powerPoolData = PowerPoolUserExtension.Create(jsonContent, apiKey);
                if (powerPoolData != null)
                {
                    // Create filter for upsert operation
                    var filter = Builders<PowerPoolUser>.Filter.Eq(u => u.ApiKey, apiKey);

                    await _powerPoolScrapingPersistency.InsertAsync(powerPoolData);
                    foreach (var minerData in powerPoolData.Miners.Values)
                    {
                        var workersStatus = minerData.Workers.GetAllWorkerStatus();
                        var workers = workersStatus.GetWorkerIdsStatus(_workerStore.GetWorkerIdMap(), powerPool);
                        _log.LogInformation("Workers from current scraping matching count: {matchCount}, added count:{unmatchCount}, removed count:{removeCount}", workers.Matching.Count(), workers.Added.Count(), workers.Removed.Count());                        
                        if (!workers.Added.IsEmpty())
                        {
                            _log.LogInformation("Adding new Workers");
                            var newWorkerStatus = workersStatus.Where(w => workers.Added.Contains(w.GetExternalId(powerPool)));
                            var newWorkers = newWorkerStatus.Select(w => w.AsNewWorker(powerPool));
                            await _workerPersistency.InsertManyAsync(newWorkers);                            
                        }
                        if (!workers.Removed.IsEmpty())
                        {
                            _log.LogInformation("Removed Workers!! {removedId} ", string.Join(',',workers.Removed));                            
                        }
                        else
                        {
                            _log.LogInformation("Workers are equal, no new workers to insert");
                        }

                    }

                    _log.LogInformation("Data for API key {apiKey} stored successfully at {fetchedAt}", apiKey, powerPoolData.FetchedAt);
                }
            }
            catch (HttpRequestException ex)
            {
                _log.LogError("Error fetching data from PowerPool API: {message} {stackTrace} ", ex.Message, ex.StackTrace);
                throw new Exception($"Error fetching data from PowerPool API: {ex.Message} {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                _log.LogError("Error fetching data from PowerPool API: {message} {stackTrace}", ex.Message, ex.StackTrace);
                throw new Exception($"Error storing data in MongoDB: {ex.Message} {ex.StackTrace}");
            }
        }
        public async Task<double> GetTodayCoverageAsync()
        {
            try
            {
                _log.LogInformation("GetTodayCoverageAsync");
                var today = DateUtils.Today;
                DateTime beginOfToday = today.ToDateTime(new TimeOnly(0, 0, 0));
                DateTime endOfToday = today.ToDateTime(new TimeOnly(23, 59, 59));
                var data = await GetDataRangeAsync(beginOfToday, endOfToday);
                var dataCount = data.Count();
                var numberOfMinutesToday = (DateTime.Now - beginOfToday).TotalMinutes;
                _log.LogInformation("Data count: {dataCount}, numberOfMinutesTillNow: {minutes}", dataCount, numberOfMinutesToday);
                var coverage = (double)dataCount / numberOfMinutesToday * 100;
                return coverage;
            }
            catch (Exception ex)
            {
                _log.LogError("Error calculating today coverage: {message}", ex.Message);
                return 0;
                throw new Exception($"Error calculating today coverage: {ex.Message}");
            }
        }
        public async Task<PowerPoolUser> GetLatestUserDataAsync() => await _powerPoolScrapingPersistency.GetLatestUserDataAsync();

        public async Task<IEnumerable<PowerPoolUser>> GetDataRangeAsync(DateTime from, DateTime to) => await _powerPoolScrapingPersistency.GetDataRangeAsync(from, to);
        public async Task<IEnumerable<ISnapshotWorkerStatus>> GetSnapshotWorkerStatusAsync(long workerId, DateTime from, DateTime to)
        {
            return await _powerPoolScrapingPersistency.GetSnapshotWorkerStatusAsync(WorkerId.Create(powerPool.PoolId,workerId),from, to);
        }
    }
}
