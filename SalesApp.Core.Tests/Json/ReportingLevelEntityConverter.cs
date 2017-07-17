using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.BL.Models.Stats.Reporting;

namespace SalesApp.Core.Tests.Json
{
    [TestFixture]
    public class ReportingLevelEntityConverter
    {
        private string json =
            "{\"ParentId\" : \"ABC09124\",\"Name\" : \"Service Center One\",\"StatsType\" : \"Service Center\",\"ReportStatsType\" : \"DSR\",\"Sales\" :[ {\"Name\" : \"TODAY\",\"Value\": \"123\"},{\"Name\" : \"THIS WEEK\",\"Value\": \"123\"},{\"Name\" : \"THIS MONTH\",\"Value\": \"123\"}],\"ReportStats\":[{\"Date\":\"9/25/2015\",\"Rank\": 1,\"Sales\": 6,\"Prospects\": 5,\"Name\": \"NAIROBI\",\"ItemId\":\"000000001\",\"Dsrs\": 9}]}";

        [Test]
        public void ConvertReportingLevelEntityTest()
        {
            ReportingLevelEntity result = JsonConvert.DeserializeObject<ReportingLevelEntity>(json);
            Assert.That(result.Name, Is.EqualTo("Service Center One"));
        }
    }
}
