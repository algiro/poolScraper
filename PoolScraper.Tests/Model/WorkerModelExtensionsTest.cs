using FluentAssertions;
using PoolScraper.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Model
{
    public class WorkerModelExtensionsTest
    {
        [TestCase("tmsminer007.8mrs21216", "S21")]
        [TestCase("tmsminer007.4l79300", "L7")]
        [TestCase("tmsminer007.1ds21xp270", "S21XP")]
        [TestCase("tmsminer007.6dg111000", "DG1")]
        [TestCase("tmsminer007.5l7930repair", "L7")]
        public void TryGetModel_ValidString_ReturnsTrueAndParsedModel(string modelName,string expectedModelName)
        {
            // Act
            bool parsed = WorkerModelExtensions.TryGetModel(modelName, out IWorkerModel parsedModel);
            // Assert
            parsed.Should().BeTrue();
            parsedModel.Name.Should().Be(expectedModelName);
        }

        [TestCase("tmsminer007.8mrs22216")]
        [TestCase("tmsminer007.4l89300")]
        [TestCase("tmsminer007.1ds22x270")]
        [TestCase("tmsminer007.6d11000")]

        public void TryGetModel_InvalidValidString_ReturnsFalse(string modelName)
        {
            // Act
            WorkerModelExtensions.TryGetModel(modelName, out IWorkerModel parsedModel).Should().BeFalse();
        }

    }
}
