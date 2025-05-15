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
        [TestCase("tmsminer007.8mrs21216",  "Myrig")]
        [TestCase("tmsminer007.1ds21xp270", "Dubai")]
        [TestCase("tmsminer007.2eths21200", "Ethiopia")]
        [TestCase("tmsminer007.eths21200",  "Ethiopia")]

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
