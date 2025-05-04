using PoolScraper.Service.Consolidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Tests.Consolidation
{
    // NUnit test fixture using FluentAssertions.
    [TestFixture]
    public class HourlySnapshotsConsolidationTests
    {
        private readonly IWorkerId workerId1 = WorkerId.Create ("PoolA", 1 );
        private readonly IWorkerId workerId2 = WorkerId.Create ("PoolA", 2 );

        [Test]
        public void GetHourlySnapshots_ShouldGroupSnapshotsByHourAndComputeWeightedAverage()
        {
            // Arrange
            var baseDate = new DateTime(2025, 1, 1);

            // Create two snapshots within the same hour (hour 0)
            // Snapshot 1: 00:15 to 00:30, weight = 15 minutes, hashrate = 100
            var snapshot1 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(15), baseDate.AddMinutes(30)),
                WorkerBasicInfo.Create(100, 0));

            // Snapshot 2: 00:30 to 00:45, weight = 15 minutes, hashrate = 200
            var snapshot2 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(30), baseDate.AddMinutes(45)),
                WorkerBasicInfo.Create(200, 0));

            var snapshots = new List<ISnapshotWorkerStatus> { snapshot1, snapshot2 };

            var consolidation = new HourlySnapshotsConsolidation();

            // Act
            var result = consolidation.GetHourlySnapshots(snapshots).ToList();

            var hour0 = result.FirstOrDefault(r => r.hour == 0);
            // Assert
            hour0.snapshots.Should().HaveCount(1);
            hour0.hour.Should().Be(0);

            // Calculate weighted average expected: (15*100 + 15*200) / (15+15) = 150
            hour0.snapshots.First().BasicInfo.Hashrate.Should().Be(150);
        }

        [Test]
        public void GetHourlySnapshots_DifferentWorkersShouldGroupSnapshotsByHourAndComputeWeightedAverage()
        {
            // Arrange
            var baseDate = new DateTime(2025, 1, 1);

            // Create two snapshots within the same hour (hour 0)
            // Snapshot 1: 00:15 to 00:30, weight = 15 minutes, hashrate = 100
            var w1_snapshot1 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(15), baseDate.AddMinutes(30)),
                WorkerBasicInfo.Create(100, 0));

            // Snapshot 2: 00:30 to 00:45, weight = 15 minutes, hashrate = 200
            var w1_snapshot2 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(30), baseDate.AddMinutes(45)),
                WorkerBasicInfo.Create(200, 0));

            // Create two snapshots within the same hour (hour 0)
            // Snapshot 1: 00:15 to 00:30, weight = 15 minutes, hashrate = 100
            var w2_snapshot1 = SnapshotWorkerStatus.Create(
                workerId2,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(15), baseDate.AddMinutes(30)),
                WorkerBasicInfo.Create(200, 0));

            // Snapshot 2: 00:30 to 00:45, weight = 15 minutes, hashrate = 200
            var w2_snapshot2 = SnapshotWorkerStatus.Create(
                workerId2,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(30), baseDate.AddMinutes(45)),
                WorkerBasicInfo.Create(300, 0));


            var snapshots = new List<ISnapshotWorkerStatus> { w1_snapshot1, w1_snapshot2, w2_snapshot1, w2_snapshot2 };

            var consolidation = new HourlySnapshotsConsolidation();

            // Act
            var result = consolidation.GetHourlySnapshots(snapshots).ToList();
            // Assert

            var hour0 = result.FirstOrDefault(r => r.hour == 0);
            // Assert
            hour0.snapshots.Should().HaveCount(2); //Two workers should be present in the result

            hour0.snapshots.Single(w => w.WorkerId.Id == 1).BasicInfo.Hashrate.Should().Be(150);
            hour0.snapshots.Single(w => w.WorkerId.Id == 2).BasicInfo.Hashrate.Should().Be(250);
        }

        [Test]
        public void GetHourlySnapshots_ShouldThrowArgumentException_WhenSnapshotsAreNotFromTheSameDay()
        {
            // Arrange
            var baseDate = new DateTime(2025, 1, 1);

            // Snapshot 1: from baseDate.
            var snapshot1 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)),
                WorkerBasicInfo.Create(100, 0));

            // Snapshot 2: from the next day.
            var snapshot2 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddDays(1), baseDate.AddDays(1).AddMinutes(15)),
                WorkerBasicInfo.Create(150, 0));

            var snapshots = new List<ISnapshotWorkerStatus> { snapshot1, snapshot2 };

            var consolidation = new HourlySnapshotsConsolidation();

            // Act
            Action act = () => consolidation.GetHourlySnapshots(snapshots).ToList();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All snapshots must be in the same day.");
        }

        [Test]
        public void GetHourlySnapshots_ShouldThrowInvalidOperationException_WhenSnapshotsOverlapMoreThanOneMinute()
        {
            // Arrange
            var baseDate = new DateTime(2025, 1, 1);

            // Create overlapping snapshots:
            // Snapshot 1: 00:00 to 00:30
            // Snapshot 2: 00:15 to 00:45  (overlap with snapshot1 is 15 minutes)
            var snapshot1 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate, baseDate.AddMinutes(30)),
                WorkerBasicInfo.Create(100, 0));

            var snapshot2 = SnapshotWorkerStatus.Create(
                workerId1,
                Granularity.Minutes,
                DateRange.Create(baseDate.AddMinutes(15), baseDate.AddMinutes(45)),
                WorkerBasicInfo.Create(200, 0));

            var snapshots = new List<ISnapshotWorkerStatus> { snapshot1, snapshot2 };

            var consolidation = new HourlySnapshotsConsolidation();

            // Act
            Action act = () => consolidation.GetHourlySnapshots(snapshots).ToList();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Snapshots have overlapping time ranges greater than one minute.");
        }
    }
}
