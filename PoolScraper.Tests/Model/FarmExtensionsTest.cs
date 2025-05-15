using FluentAssertions;
using PoolScraper.Config;
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
        [SetUp]
        public void SetUp()
        {
            var stringConfig = "1|Russia1|^\\d*mr|Russia;2|Dubai1|^\\d*d|Dubai;3|Ethiopia1|^\\d*eth|Ethiopia";
            Farm.UpdateStore(PoolScraperConfig.GetFarms(stringConfig));
        }
        [TestCase("tmsminer007.8mrs21216",  "Russia1")]
        [TestCase("tmsminer007.1ds21xp270", "Dubai1")]
        [TestCase("tmsminer007.2eths21200", "Ethiopia1")]
        [TestCase("tmsminer007.eths21200", "Ethiopia1")]

        public void TryGetModel_ValidString_ReturnsTrueAndParsedModel(string modelName,string expectedFarmName)
        {
            // Act
            bool parsed = Farm.TryGetFarm(modelName, out var parsedFarm);
            // Assert
            parsed.Should().BeTrue();
            parsedFarm.Name.Should().Be(expectedFarmName);
        }

        [TestCase("tmsminer007.4l79300", "UNKNOWN")]
        [TestCase("tmsminer007.l79300", "UNKNOWN")]
        [TestCase("tmsminer007.1mtl79300", "UNKNOWN")]

        public void TryGetModel_InvalidString_ReturnsFalse(string modelName, string expectedFarmId)
        {
            Farm.TryGetFarm(modelName, out var parsedFarm).Should().BeFalse();
        }
    }
}
