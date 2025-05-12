using CommonUtils.Utils.Logs;
using FluentAssertions;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service;
using PoolScraper.Service.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Model.PowerPool
{
    public class WorkerStatusExtensionTest
    {
        private static readonly IPool pool = Pool.CreatePowerPool();
        private const int ID1= 123456;
        private const int ID2= 654321;
        private readonly IWorker  worker1 = Worker.Create(pool.PoolId, "SHA256", ID1, "Worker1");
        private readonly IWorker  worker2 = Worker.Create(pool.PoolId, "SHA256", ID2, "Worker2");
        private WorkerStatus workerStatus1;
        private WorkerStatus workerStatus2;
        [SetUp]
        public void SetUp()
        {
            workerStatus1 = new WorkerStatus()
            {
                Id = worker1.WorkerId.Id,
                Algorithm = "SHA256",
                Name = "Worker1",
                ValidShares = 100,
                InvalidShares = 5,
                StaleShares = 2,
                Blocks = 1,
                Hashrate = 1000.0,
                HashrateUnits = "MH/s",
                HashrateAvg = 950.0,
                HashrateAvgUnits = "MH/s"
            };
            workerStatus2 = new WorkerStatus()
            {
                Id = worker2.WorkerId.Id,
                Algorithm = "SHA256",
                Name = "Worker2",
                ValidShares = 200,
                InvalidShares = 10,
                StaleShares = 4,
                Blocks = 2,
                Hashrate = 2000.0,
                HashrateUnits = "MH/s",
                HashrateAvg = 1900.0,
                HashrateAvgUnits = "MH/s"
            };

        }

        [Test]
        public void AsWorkerMinuteStatus_NoIdMatchDefined()
        {
            var dateRange = DateRange.Create(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
            var workerStatus = new WorkerStatus()
            {
                Id = ID1,
                Algorithm = "SHA256",
                Name = "Worker1",
                ValidShares = 100,
                InvalidShares = 5,
                StaleShares = 2,
                Blocks = 1,
                Hashrate = 1000.0,
                HashrateUnits = "MH/s",
                HashrateAvg = 950.0,
                HashrateAvgUnits = "MH/s"
            };
            var snap = workerStatus.AsWorkerMinuteStatus(pool, dateRange);
            snap.WorkerId.Id.Should().Be(ID1);
            snap.WorkerId.PoolId.Should().Be(pool.PoolId);
        }
        [Test]
        public void AsWorkerMinuteStatus_WithIdMatchDefined()
        {
            var pool = Pool.CreatePowerPool();
            Dictionary<IExternalId, IWorkerId> workerIdMap = new Dictionary<IExternalId, IWorkerId>()
            {
                [ExternalId.Create(pool.PoolId, ID1)] = WorkerId.Create(pool.PoolId, ID2)
            };
            WorkerIdMap.Initialize(workerIdMap);

            var dateRange = DateRange.Create(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
            var workerStatus = new WorkerStatus()
            {
                Id = ID1,
                Algorithm = "SHA256",
                Name = "Worker1",
                ValidShares = 100,
                InvalidShares = 5,
                StaleShares = 2,
                Blocks = 1,
                Hashrate = 1000.0,
                HashrateUnits = "MH/s",
                HashrateAvg = 950.0,
                HashrateAvgUnits = "MH/s"
            };
            var snap = workerStatus.AsWorkerMinuteStatus(pool, dateRange);
            snap.WorkerId.Id.Should().Be(654321);
            snap.WorkerId.PoolId.Should().Be(pool.PoolId);
        }

        [Test]
        public void AsWorkersMinuteStatus_VerifyAggregation()
        {
            var pool = Pool.CreatePowerPool();
            // mapping worker2 to worker1 (like an alias)
            Dictionary<IExternalId, IWorkerId> workerIdMap = new Dictionary<IExternalId, IWorkerId>()
            {
                [ExternalId.Create(pool.PoolId, worker2.WorkerId.Id)] = WorkerId.Create(pool.PoolId, worker1.WorkerId.Id)
            };
            WorkerIdMap.Initialize(workerIdMap);

            var startingTime = new DateTime(2025,01,01, 12,15,00, DateTimeKind.Utc);
            var dateRange1 = DateRange.Create(startingTime, startingTime.AddMinutes(1));
            var dateRange2 = DateRange.Create(startingTime.AddMinutes(1), startingTime.AddMinutes(2));

            var workersStatusList = new List<WorkerStatus> { workerStatus1, workerStatus2 };
            var snap1 = workerStatus1.AsWorkerMinuteStatus(pool, dateRange1);
            var snap2 = workerStatus2.AsWorkerMinuteStatus(pool, dateRange2);
            var snapsList = new ISnapshotWorkerStatus[] { snap1, snap2 };

            var workerStore = new WorkerStore(LoggerUtils.CreateLogger(""), new List<IWorker> { worker1, worker2 });
            var snapsListDetails = snapsList.Select(s => s.AsSnapshotDetailedView(workerStore));
            var report = new WorkersReport();
            var averagePerModel = report.CalculateAveragePerModel(snapsListDetails);

            averagePerModel.Should().HaveCount(1);
            averagePerModel.First().WorkerId.Id.Should().Be(worker1.WorkerId.Id);
            averagePerModel.First().BasicInfo.Hashrate.Should().Be(1500);

        }
        // after aggregation, same worker in the same dateRange
        [Test]
        public void AsWorkersMinuteStatus_CheckDuplicatedEntries()
        {
            var pool = Pool.CreatePowerPool();
            // mapping worker2 to worker1 (like an alias)
            Dictionary<IExternalId, IWorkerId> workerIdMap = new Dictionary<IExternalId, IWorkerId>()
            {
                [ExternalId.Create(pool.PoolId, worker2.WorkerId.Id)] = WorkerId.Create(pool.PoolId, worker1.WorkerId.Id)
            };
            WorkerIdMap.Initialize(workerIdMap);

            var startingTime = new DateTime(2025, 01, 01, 12, 15, 00, DateTimeKind.Utc);
            var dateRange1 = DateRange.Create(startingTime, startingTime.AddMinutes(1));

            var workersStatusList = new List<WorkerStatus> { workerStatus1, workerStatus2 };
            Action act = () => workersStatusList.AsWorkersMinuteStatus(pool, dateRange1);
            act.Should().Throw<ArgumentException>()
                .WithMessage("The snapshot contains multiple entries for the same workerId in the same date range.");

        }
    }
}
