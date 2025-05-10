using CommonUtils.Utils;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
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
        private IEnumerable<IWorker> _allWorkers = Enumerable.Empty<IWorker>();
        private readonly IPool powerPool = Pool.CreatePowerPool();
        private readonly IPowerPoolScrapingPersistency _powerPoolScrapingPersistency;
        private readonly IWorkerPersistency _workerPersistency;

        public PowePoolScrapingService(ILogger<PowePoolScrapingService> log, IPowerPoolScrapingPersistency powerPoolScrapingPersistency, IWorkerPersistency workerPersistency)
        {
            _log = log;
            _httpClient = new HttpClient();
            _powerPoolScrapingPersistency = powerPoolScrapingPersistency;
            _workerPersistency = workerPersistency;
        }
        private string apiKey => powerPool.ApiKey;
        public async Task FetchAndStoreUserData()
        {
            try
            {
                if (_areWorkersLoaded.CheckIfCalledAndSet)
                {
                    _allWorkers = await _workerPersistency.GetAllWorkerAsync();
                    _log.LogInformation("FetchAndStoreUserData, first loading of allWorkers# {count}", _allWorkers.Count());
                }
                else
                {
                    _log.LogInformation("AllWorkers already loaded");
                }

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
                        var workers = minerData.Workers.GetAllWorkerStatus().Select(w => Worker.Create(PoolIDs.PowerPool, w.Algorithm, w.Id, w.Name)).OrderBy(w => w.WorkerId);
                        _log.LogInformation("Workers from current scraping count: {count}", workers.Count());
                        bool areEqual = workers.ToList().SequenceEqual(_allWorkers);
                        if (!areEqual)
                        {
                            _log.LogInformation("Workers are not equal, inserting new workers");
                            var newWorkers = workers.Except(_allWorkers).ToList();
                            _log.LogInformation("Identified #newWorkers: {newWorkersCount} ", newWorkers.Count);
                            await _workerPersistency.InsertManyAsync(newWorkers);
                            _allWorkers = await _workerPersistency.GetAllWorkerAsync();
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
