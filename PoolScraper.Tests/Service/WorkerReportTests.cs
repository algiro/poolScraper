using PoolScraper.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Tests.Service
{
    [TestFixture]
    public class WorkersReportTests
    {
        [Test]
        public void CalculateAveragePerWorker_WithMultipleSnapshotsForOneWorker_ReturnsSingleSnapshotWithCorrectAverage()
        {
            // Arrange
            var worker = Worker.Create("pool1", "alg1", 1, "worker1");
            var snapshots = new List<ISnapshotDetailedView> {
              CreateSnapshot(worker, 10, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1)), // Hashrate 10, 1 hour
              CreateSnapshot(worker, 20, DateTime.Now.AddHours(-1), DateTime.Now),  // Hashrate 20, 1 hour
              };

            var workersReport = new SnapshotWorkersReport();

            // Act
            var result = workersReport.CalculateAveragePerWorker(snapshots).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].BasicInfo.Hashrate.Should().BeApproximately(15, 0.001); // Weighted average: (10*1 + 20*1) / 2 = 15
            result[0].Granularity.Should().Be(Granularity.Custom);
            result[0].DateRange.From.Should().Be(snapshots.Min(s => s.DateRange.From));
            result[0].DateRange.To.Should().Be(snapshots.Max(s => s.DateRange.To));
            result[0].Worker.Should().Be(worker); // Verify the worker is the same
        }

        [Test]
        public void CalculateAveragePerWorker_WithMultipleWorkers_ReturnsCorrectAveragesForEachWorker()
        {
            // Arrange
            var worker1 = Worker.Create("pool1", "alg1", 1, "worker1");
            var worker2 = Worker.Create("pool1", "alg1", 2, "worker2");

            var snapshots = new List<ISnapshotDetailedView> {
                CreateSnapshot(worker1, 10, DateTime.Now.AddHours(-1), DateTime.Now),
                CreateSnapshot(worker1, 20, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1)),
                CreateSnapshot(worker2, 30, DateTime.Now.AddHours(-1), DateTime.Now),
                CreateSnapshot(worker2, 40, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1))
            };
            var workersReport = new SnapshotWorkersReport();
            // Act
            var result = workersReport.CalculateAveragePerWorker(snapshots).ToList();

            // Assert
            result.Should().HaveCount(2);

            var avgWorker1 = result.FirstOrDefault(x => x.Worker.WorkerId.Id == worker1.WorkerId.Id);
            var avgWorker2 = result.FirstOrDefault(x => x.Worker.WorkerId.Id == worker2.WorkerId.Id);

            avgWorker1.Should().NotBeNull();
            avgWorker2.Should().NotBeNull();

            avgWorker1.BasicInfo.Hashrate.Should().BeApproximately(15, 0.001); // (10*1 + 20*1) / 2
            avgWorker2.BasicInfo.Hashrate.Should().BeApproximately(35, 0.001); // (30*1 + 40*1) / 2
        }

        [Test]
        public void CalculateAveragePerWorker_WithNoSnapshots_ReturnsEmptyList()
        {
            // Arrange
            var snapshots = new List<ISnapshotDetailedView>();
            var workersReport = new SnapshotWorkersReport();

            // Act
            var result = workersReport.CalculateAveragePerWorker(snapshots).ToList();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void CalculateAveragePerWorker_WithSnapshotHavingZeroWeight_ReturnsValidAverage()
        {
            // Arrange
            var worker = Worker.Create("pool1", "alg1", 1, "worker1");
            var snapshots = new List<ISnapshotDetailedView> {
              CreateSnapshot(worker, 10, DateTime.Now.AddHours(-1), DateTime.Now), // 1 hour
              CreateSnapshot(worker, 20, DateTime.Now, DateTime.Now),  // 0 hour
             };

            var workersReport = new SnapshotWorkersReport();

            // Act
            var result = workersReport.CalculateAveragePerWorker(snapshots).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].BasicInfo.Hashrate.Should().BeApproximately(10, 0.001); // Weighted average: (10*1 + 20*0) / 1 = 10
        }

        // "tmsminer007.8mrs21216", WorkerModel.S21
        // "tmsminer007.4l79300", WorkerModel.L7
        // "tmsminer007.9l79300", WorkerModel.L7

        [Test]
        public void CalculateAveragePerModel_WithMultipleWorkers_ReturnsCorrectAveragesForEachWorker()
        {
            // Arrange
            var worker1 = Worker.Create("pool1", "alg1", 1, "tmsminer007.8mrs21216");
            var worker2 = Worker.Create("pool1", "alg1", 2, "tmsminer007.4l79300");
            var worker3 = Worker.Create("pool1", "alg1", 2, "tmsminer007.9l79300");

            var snapshots = new List<ISnapshotDetailedView> {
                CreateSnapshot(worker1, 10, DateTime.Now.AddHours(-1), DateTime.Now),
                CreateSnapshot(worker1, 20, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1)),
                CreateSnapshot(worker2, 30, DateTime.Now.AddHours(-1), DateTime.Now),
                CreateSnapshot(worker2, 40, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1)),
                CreateSnapshot(worker3, 50, DateTime.Now.AddHours(-1), DateTime.Now),
                CreateSnapshot(worker3, 60, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1))

            };
            var workersReport = new SnapshotWorkersReport();
            // Act
            var result = workersReport.CalculateAveragePerWorker(snapshots).ToList();

            // Assert
            result.Should().HaveCount(2);

            var avgWorkerS21 = result.FirstOrDefault(x => x.Worker.Model.Name == "S21");
            var avgWorkerL7 = result.FirstOrDefault(x => x.Worker.Model.Name == "L7");

            avgWorkerS21.Should().NotBeNull();
            avgWorkerL7.Should().NotBeNull();

            avgWorkerS21.BasicInfo.Hashrate.Should().BeApproximately(15, 0.001); // (10*1 + 20*1) / 2
            avgWorkerL7.BasicInfo.Hashrate.Should().BeApproximately(45, 0.001); // (30*1 + 40*1 + 50*1 + 60*1) / 4
        }


        private ISnapshotDetailedView CreateSnapshot(IWorker worker, double hashrate, DateTime from, DateTime to)
        {
            var workerId = worker.WorkerId;
            var dateRange = DateRange.Create(from, to);
            var basicInfo = WorkerBasicInfo.Create(hashrate, 0);
            var snapshotStatus = SnapshotWorkerStatus.Create(workerId, Granularity.Custom, dateRange, basicInfo);
            var detailView = SnapshotDetailedView.AsSnapshotDetailedView(snapshotStatus, worker);
            if (detailView == null)
            {
                throw new InvalidOperationException("Failed to create ISnapshotDetailedView");
            }
            return detailView;
        }
    }
}
