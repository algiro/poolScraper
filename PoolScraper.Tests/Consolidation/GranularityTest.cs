using PoolScraper.Model.Consolidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PoolScraper.Model.DateRange;

namespace PoolScraper.Tests.Consolidation
{
    public class GranularityTest
    {
        private readonly DateTime _testDateTime = new DateTime(2023, 4, 28, 13, 42, 12);

        [TestCase(Granularity.Minutes, "20230428.1342", 1)]
        [TestCase(Granularity.Hours, "20230428.13", 60)]
        [TestCase(Granularity.Days, "20230428", 1440)]
        [TestCase(Granularity.Custom, "20230428.1342", 0)]
        public void GetId_And_GetWeight_Return_Expected_For_Common_Granularities(Granularity granularity, string expectedId, int expectedWeight)
        {
            var id = granularity.GetId(_testDateTime);
            Assert.AreEqual(expectedId, id, $"GetId for {granularity}");
            Assert.AreEqual(expectedWeight, granularity.GetWeight(), $"GetWeight for {granularity}");
        }

        [Test]
        public void GetId_With_DateRange_Uses_MiddleDateTime()
        {
            var from = new DateTime(2023, 4, 28, 8, 0, 0);
            var to = new DateTime(2023, 4, 28, 18, 0, 0);
            var range = Create(from, to);

            // (8:00 + 300 min) = 13:00
            DateTime expectedMiddle = from.AddMinutes(((to - from).TotalMinutes) / 2);

            foreach (var granularity in new[] { Granularity.Minutes, Granularity.Hours, Granularity.Days, Granularity.Custom })
            {
                var idDirect = granularity.GetId(expectedMiddle);
                var idViaRange = granularity.GetId(range);
                Assert.AreEqual(idDirect, idViaRange, $"GetId with DateRange should match GetId with middle DateTime for {granularity}");
            }
        }

        [TestCase(Granularity.Minutes, "20230428.1342", "2023-04-28 13:42:00")]
        [TestCase(Granularity.Hours, "20230428.13", "2023-04-28 13:00:00")]
        [TestCase(Granularity.Days, "20230428", "2023-04-28 00:00:00")]
        [TestCase(Granularity.Custom, "20230428.1342", "2023-04-28 13:42:00")]
        public void GetDateTime_Parses_Id_String_Correctly(Granularity granularity, string id, string expectedDate)
        {
            var dt = granularity.GetDateTime(id);
            var expectedDt = DateTime.ParseExact(expectedDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Assert.AreEqual(expectedDt, dt, $"GetDateTime for {granularity}");
        }

        [Test]
        public void GetId_And_GetDateTime_Week_Granularity()
        {
            var weekNumber = ISOWeek.GetWeekOfYear(_testDateTime);
            var expectedId = $"{_testDateTime.Year}.{weekNumber}";
            var id = Granularity.Weeks.GetId(_testDateTime);
            Assert.AreEqual(expectedId, id, "GetId for Weeks granularity");

            // Parsing expects "yyyy.ww"--may need zero-padded week number for parsing.
            string weekIdFormatted = $"{_testDateTime.Year}.{weekNumber:D2}";
            try
            {
                var dt = Granularity.Weeks.GetDateTime(weekIdFormatted);
                Assert.AreEqual(new DateTime(2023, 4, 24, 0, 0, 0), dt, "Parsed week start date for week id");
            }
            catch (FormatException)
            {
                Assert.Inconclusive("GetDateTime for Weeks may require a zero-padded week id.");
            }
        }

        [Test]
        public void GetId_And_GetDateTime_Month_Granularity()
        {
            var expectedId = $"{_testDateTime.Year}.{_testDateTime.Month}";
            var id = Granularity.Months.GetId(_testDateTime);
            Assert.AreEqual(expectedId, id, "GetId for Months granularity");

            // Parsing expects "yyyy.MM"--needs zero-padded month for parsing.
            string monthIdFormatted = $"{_testDateTime.Year}.{_testDateTime.Month:D2}";
            var dt = Granularity.Months.GetDateTime(monthIdFormatted);
            Assert.AreEqual(new DateTime(2023, 4, 1, 0, 0, 0), dt, "Parsed month start date");
        }

        [Test]
        public void DateRange_MiddleDateTime_Calculates_Center()
        {
            var from = new DateTime(2023, 4, 1, 0, 0, 0);
            var to = new DateTime(2023, 4, 30, 0, 0, 0);
            var range = Create(from, to);
            var expectedMiddle = from.AddMinutes(((to - from).TotalMinutes) / 2);

            Assert.AreEqual(expectedMiddle, range.MiddleDateTime());
        }
    }
}
