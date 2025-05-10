using FluentAssertions;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Persistency;
using PoolScraper.Service;
using PoolScraper.Service.Store;
using PoolScraper.Tests.Utils;
using PoolScraper.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Service
{
    public class PowerPoolDataExtractorTest
    {
        private IEnumerable<IWorker> allWorkers;
        private WorkerStore workerStore;

        [SetUp]
        public async Task SetUp()
        {
            /*            var loggerFactory = (ILoggerFactory)new LoggerFactory();
                        var logger = loggerFactory.CreateLogger<WorkerStore>();
                        WorkerPersistency workerPersistency = new WorkerPersistency(logger, "mongodb://localhost:27017", "PowerPoolDB");
                        workerStore = new WorkerStore(logger, workerPersistency);
                        await workerStore.LoadAllWorkerAsync();
                        var allWorkers = workerStore.GetAllWorker();
                        var workerDTO = allWorkers.Select(w => w.ToWorkerDTO(false));
                        File.WriteAllText("c:\\temp\\worker.json", JsonConvert.SerializeObject(workerDTO));*/
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            var logger = loggerFactory.CreateLogger<WorkerStore>();

            var workers = JsonConvert.DeserializeObject<IEnumerable<WorkerDTO>>(File.ReadAllText("./Resources/workers.json"));
            allWorkers = workers.Select(w => Worker.Create(w.WorkerId.PoolId, w.Algorithm, w.WorkerId.Id, w.Name));
            workerStore = new WorkerStore(logger,allWorkers);
        }

        [Test]
        public void CalculateAveragePerWorker()
        {
            var powerPoolDataStream = ResourceHelper.Open7ZipStream("./Resources/PowerPoolData.7z");
            StreamReader reader = new StreamReader(powerPoolDataStream);
            string jsonContent = reader.ReadToEnd();
            var scrapings = JsonConvert.DeserializeObject<IEnumerable<PowerPoolUser>>(jsonContent);
            scrapings.Should().HaveCount(1377);
            var snapshots = scrapings.AsSnapshotWorkerStatus();
            snapshots.Should().HaveCount(154224);
            var snapshotDetails = snapshots.Select( s => s.AsSnapshotDetailedView(workerStore)).ToArray();

            var worker4Test = Worker.Create("pool1", "alg1", 1, "worker1");
            var workerId4602360 = snapshotDetails.Where(s => s.WorkerId.Id == 4602360);
            var hashRate4602360 = workerId4602360.Average(w => w.BasicInfo.Hashrate);
            WorkersReport workersReport = new WorkersReport();
            var averagePerWorker = workersReport.CalculateAveragePerWorker(workerId4602360);
            averagePerWorker.Should().HaveCount(1);
            averagePerWorker.ElementAt(0).BasicInfo.Hashrate.Should().BeApproximately(hashRate4602360, 0.001);

            var averagePerModelAndDate = workersReport.CalculateAveragePerModelAndDate(snapshotDetails);
            var averagePerL9 = averagePerModelAndDate.Where(a => a.Worker.Model == WorkerModel.L9);

        }
    }
}
