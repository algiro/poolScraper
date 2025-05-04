using FluentAssertions;
using PoolScraper.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Model
{
    public class FarmExtensionsTest
    {
        [TestCase("tmsminer007.8mrs21216", Farm.Myrig)]
        [TestCase("tmsminer007.1ds21xp270", Farm.Dubai)]
        [TestCase("tmsminer007.2eths21200", Farm.Ethiopia)]
        [TestCase("tmsminer007.eths21200", Farm.Ethiopia)]

        public void TryGetModel_ValidString_ReturnsTrueAndParsedModel(string modelName,Farm expectedFarm)
        {
            // Act
            bool parsed = FarmExtension.TryGetFarm(modelName, out Farm parsedFarm);
            // Assert
            parsed.Should().BeTrue();
            parsedFarm.Should().Be(expectedFarm);
        }

        [TestCase("tmsminer007.4l79300", Farm.UNKNOWN)]
        [TestCase("tmsminer007.l79300", Farm.UNKNOWN)]
        [TestCase("tmsminer007.1mtl79300", Farm.UNKNOWN)]

        public void TryGetModel_InvalidString_ReturnsFalse(string modelName, Farm expectedFarm)
        {
            FarmExtension.TryGetFarm(modelName, out Farm parsedFarm).Should().BeFalse();
        }
    }
}
