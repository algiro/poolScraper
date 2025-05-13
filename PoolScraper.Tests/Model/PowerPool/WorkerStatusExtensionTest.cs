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
        private const long WORKER_ID1= 1;
        private const long WORKER_ID2= 2;

        private const long PowPool_ID1 = 165342;
        private const long PowPool_ID2 = 243241;
        private const long PowPool_ID3 = 546783;

        private readonly IWorker  worker1 = Worker.Create(pool.PoolId, "SHA256", WORKER_ID1, "Worker1");
        private readonly IWorker  worker2 = Worker.Create(pool.PoolId, "SHA256", WORKER_ID2, "Worker2");
        private WorkerStatus workerStatus1;
        private WorkerStatus workerStatus2;
        private WorkerStatus workerStatus3;
        [SetUp]
        public void SetUp()
        {
            workerStatus1 = new WorkerStatus()
            {
                Id = PowPool_ID1,
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
                Id = PowPool_ID2,
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
            workerStatus3 = new WorkerStatus()
            {
                Id = PowPool_ID3,
                Algorithm = "SHA256",
                Name = "Worker3",
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
            var snap = workerStatus1.AsWorkerMinuteStatus(WorkerIdMap.Create(), pool, dateRange);
            snap.Should().BeNull();
        }
        [Test]
        public void AsWorkerMinuteStatus_WithIdMatchDefined()
        {
            var pool = Pool.CreatePowerPool();
            Dictionary<IExternalId, IWorkerId> workerIdDic = new Dictionary<IExternalId, IWorkerId>()
            {
                [workerStatus1.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1)
            };
            var workerIdMap = CreateWorkerIdMap(workerIdDic);

            var dateRange = DateRange.Create(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
            var snap = workerStatus1.AsWorkerMinuteStatus(workerIdMap, pool, dateRange);
            snap.WorkerId.Id.Should().Be(WORKER_ID1);
            snap.WorkerId.PoolId.Should().Be(pool.PoolId);
        }


        [Test]
        public void AsWorkersMinuteStatus_VerifyAggregation()
        {
            var pool = Pool.CreatePowerPool();
            // mapping worker2 to worker1 (like an alias)
            Dictionary<IExternalId, IWorkerId> workerIdDic = new Dictionary<IExternalId, IWorkerId>()
            {
                [workerStatus1.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1),
                [workerStatus2.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1)
            };
            var workerIdMap = CreateWorkerIdMap(workerIdDic);

            var startingTime = new DateTime(2025,01,01, 12,15,00, DateTimeKind.Utc);
            var dateRange1 = DateRange.Create(startingTime, startingTime.AddMinutes(1));
            var dateRange2 = DateRange.Create(startingTime.AddMinutes(1), startingTime.AddMinutes(2));

            var workersStatusList = new List<WorkerStatus> { workerStatus1, workerStatus2 };
            var snap1 = workerStatus1.AsWorkerMinuteStatus(workerIdMap, pool, dateRange1);
            var snap2 = workerStatus2.AsWorkerMinuteStatus(workerIdMap, pool, dateRange2);
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
            Dictionary<IExternalId, IWorkerId> workerIdDic = new Dictionary<IExternalId, IWorkerId>()
            {
                [workerStatus1.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1),
                [workerStatus2.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1)
            };
            var workerIdMap = CreateWorkerIdMap(workerIdDic);

            var startingTime = new DateTime(2025, 01, 01, 12, 15, 00, DateTimeKind.Utc);
            var dateRange1 = DateRange.Create(startingTime, startingTime.AddMinutes(1));

            var workersStatusList = new List<WorkerStatus> { workerStatus1, workerStatus2 };
            Action act = () => workersStatusList.AsWorkersMinuteStatus(workerIdMap,pool, dateRange1);
            act.Should().Throw<ArgumentException>()
                .WithMessage("The snapshot contains multiple entries for the same workerId in the same date range.");

        }
        /* 
         * Existing map:
         *     PowPool_ID1 => WORKER_ID1
         *     PowPool_ID2 => WORKER_ID2
         * Current WorkerStatus:
         *     PowPool_ID1, PowPool_ID2
         *     
         * Expected: no changes
         */
        [Test]               
        public void GetWorkerIds_FullMatch()
        {
            var pool = Pool.CreatePowerPool();
            // mapping worker2 to worker1 (like an alias)
            Dictionary<IExternalId, IWorkerId> workerIdDic = new Dictionary<IExternalId, IWorkerId>()
            {
                [workerStatus1.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1),
                [workerStatus2.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID2)
            };
            var workerIdMap = CreateWorkerIdMap(workerIdDic);
            var workersStatusList = new List<WorkerStatus> { workerStatus1, workerStatus2 };
            var idStatus = workersStatusList.GetWorkerIdsStatus(workerIdMap, pool);
            idStatus.Added.Should().BeEmpty();
            idStatus.Removed.Should().BeEmpty();
            idStatus.Matching.Should().HaveCount(2);
        }

        /* 
         * Existing map:
         *     PowPool_ID1 => WORKER_ID1
         *     PowPool_ID2 => WORKER_ID2
         * Current WorkerStatus:
         *     PowPool_ID1, PowPool_ID2, PowPool_ID3
         *     
         * Expected: Added PowPool_ID3, Removed: nothing, Matching: PowPool_ID1, PowPool_ID2
         */
        [Test]
        public void GetWorkerIds_OneAdded()
        {
            var pool = Pool.CreatePowerPool();
            // mapping worker2 to worker1 (like an alias)
            Dictionary<IExternalId, IWorkerId> workerIdDic = new Dictionary<IExternalId, IWorkerId>()
            {
                [workerStatus1.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1),
                [workerStatus2.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID2)
            };
            var workerIdMap = CreateWorkerIdMap(workerIdDic);
            var workersStatusList = new List<WorkerStatus> { workerStatus1, workerStatus2, workerStatus3 };
            var idStatus = workersStatusList.GetWorkerIdsStatus(workerIdMap, pool);
            idStatus.Added.Should().HaveCount(1);
            idStatus.Added.First().Id.Should().Be(PowPool_ID3.ToString());
            idStatus.Removed.Should().BeEmpty();
            idStatus.Matching.Should().HaveCount(2);
        }

        /* 
             * Existing map:
             *     PowPool_ID1 => WORKER_ID1
             *     PowPool_ID2 => WORKER_ID2
             * Current WorkerStatus:
             *     PowPool_ID2, PowPool_ID3
             *     
             * Expected: Added PowPool_ID3, Removed: PowPool_ID1, Matching: PowPool_ID2
             */
        [Test]
        public void GetWorkerIds_OneAddedOneRemoved()
        {
            var pool = Pool.CreatePowerPool();
            // mapping worker2 to worker1 (like an alias)
            Dictionary<IExternalId, IWorkerId> workerIdDic = new Dictionary<IExternalId, IWorkerId>()
            {
                [workerStatus1.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID1),
                [workerStatus2.GetExternalId(pool)] = WorkerId.Create(pool.PoolId, WORKER_ID2)
            };
            var workerIdMap = CreateWorkerIdMap(workerIdDic);
            var workersStatusList = new List<WorkerStatus> { workerStatus2, workerStatus3 };
            var idStatus = workersStatusList.GetWorkerIdsStatus(workerIdMap, pool);
            idStatus.Added.Should().HaveCount(1);
            idStatus.Added.First().Id.Should().Be(PowPool_ID3.ToString());
            idStatus.Removed.Should().HaveCount(1);
            idStatus.Removed.First().Id.Should().Be(WORKER_ID1);

            idStatus.Matching.Should().HaveCount(1);
        }
        private IWorkerIdMap CreateWorkerIdMap(Dictionary<IExternalId, IWorkerId> workerIdMap)
        {
            return WorkerIdMap.Create(workerIdMap);
        }

    }
}
