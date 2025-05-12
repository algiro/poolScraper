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
        
        [TestCase("tmsminer007.8mrs21216", 216)]
        [TestCase("tmsminer007.4l79300", 9300)]
        [TestCase("tmsminer007.1ds21xp270", 270)]
        [TestCase("tmsminer007.6dg111000", 11000)]
        public void TryGetNominalHashRate_ValidString_ReturnsTrueAndParsedHashRate(string modelName, long expectedHashRate)
        {
            // Act
            bool parsed = WorkerModelExtensions.TryGetNominalHashRate(modelName, out var hashRate);
            // Assert
            parsed.Should().BeTrue();
            hashRate.Should().Be(expectedHashRate);
        }

        [TestCase("tmsminer007.5l7930repair")]

        public void TryGetNominalHashRate_InvalidValidString_ReturnsFalse(string modelName)
        {
            // Act
            WorkerModelExtensions.TryGetNominalHashRate(modelName, out var hashRate).Should().BeFalse();
        }

    }
}
