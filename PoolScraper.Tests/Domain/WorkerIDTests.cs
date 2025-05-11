using FluentAssertions;
using PoolScraper.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Domain
{
    public class WorkerIDTests
    {
        [Test]
        public void TestEquality1()
        {
            var workerId1 = PoolScraper.Domain.WorkerId.Create("pool1", 1);
            var workerId2 = PoolScraper.Domain.WorkerId.Create("pool1", 1);
            var workerId3 = PoolScraper.Domain.WorkerId.Create("pool2", 2);
            workerId1.Should().Be(workerId2);
            workerId1.Should().NotBe(workerId3);
        }

        [Test]
        public void TestEquality2()
        {
            var workerIdDTO1 = new WorkerIdDTO("pool1", 1);
            var workerId2    = PoolScraper.Domain.WorkerId.Create("pool1", 1);
            var workerId3    = PoolScraper.Domain.WorkerId.Create("pool2", 2);
            var workerIdDTO3 = new WorkerIdDTO("pool1", 1);

            workerIdDTO1.AsWorkerId().Should().Be(workerId2);
            workerIdDTO1.AsWorkerId().Should().NotBe(workerId3);
            workerIdDTO1.AsWorkerId().Should().Be(workerIdDTO3.AsWorkerId());
        }

    }
}
