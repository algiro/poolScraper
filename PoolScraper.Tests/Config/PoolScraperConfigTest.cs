using FluentAssertions;
using PoolScraper.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Config
{
    public class PoolScraperConfigTest
    {
        [Test]
        public void CreateFarmFromConfig_VeriftIsCompatible_Success()
        {
            var stringConfig = "1|Russia1|^\\d*mr|Russia;2|Dubai1|^\\d*d|Dubai;3|Ethiopia1|^\\d*eth|Ethiopia";
            var farms = PoolScraperConfig.GetFarms(stringConfig);
            farms.Should().HaveCount(3);
            var farm1 = farms.FirstOrDefault(f => f.Id == 1);
            var farm2 = farms.FirstOrDefault(f => f.Id == 2);
            var farm3 = farms.FirstOrDefault(f => f.Id == 3);
                        
            farm1.IsCompatible("tmsminer007.8mrs21216").Should().BeTrue();
            farm2.IsCompatible("tmsminer007.1ds21xp270").Should().BeTrue();
            farm3.IsCompatible("tmsminer007.2eths21200").Should().BeTrue();
        }

    }
}
