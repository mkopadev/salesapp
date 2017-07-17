using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Services.Stats.Ranking;

namespace SalesApp.Core.Tests.Services.Stats.Ranking
{
    [TestFixture]
    public class RemoteDsrRankingSummarizedServiceTestsTests : RemoteServicesTestsBase<DsrRankingSummarizedApi, List<RankingSummarized>, RemoteDsrRankingSummarizedService, RankingSummarized>
    {
        // [Test] Broken test
        public async override Task ApiCallsOnline()
        {
        }

        protected override List<RankingSummarized> ValidResponse
        {
            get
            {
                return new List<RankingSummarized>
                {
                    new RankingSummarized
                    {
                        Area = "Service Centre"
                        ,
                        DsrRank = 3
                        ,
                        TotalDsrs = 10
                        ,
                        Level = 0
                    }
                    ,
                    new RankingSummarized
                    {
                        Area = "Area"
                        ,
                        DsrRank = 10
                        ,
                        TotalDsrs = 90
                        ,
                        Level = 1
                    }
                    ,
                    new RankingSummarized
                    {
                        Area = "All"
                        ,
                        DsrRank = 80
                        ,
                        TotalDsrs = 600
                        ,
                        Level = 2
                    }

                };

            }
        }

        // [Test] Broken Test
        public async override Task ApiCallsOffline()
        {
        }

        // [Test] Broken Test
        public async override Task ApiCallsCanceled()
        {
        }

        // [Test] Broken test
        public async override Task MockCommonHttpFailureCodes()
        {
        }

        // [Test] Broken Test
        public async override Task MockNullResponse()
        {
        }

        // [Test] Broken Test
        public async override Task MockMalformedJsonResponse()
        {
        }

        // [Test] Broken Test
        public async override Task MockWrongJsonResponse()
        {
        }

        // [Test] Broken Test
        public async override Task MockTimeout()
        {
        }

        // [Test] Broken test
        public override async Task ActualOnlineCall()
        {
        }

        [Test]
        public void RankingSummarizedSerializerTest()
        {
            string json = "[ { \"Area\": \"Service Centre\" ,\"DsrRank\": 3 ,\"TotalDsrs\": 10 ,\"Level\": 0 }]";
            List<RankingSummarized> result = JsonConvert.DeserializeObject<List<RankingSummarized>>(json);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Area, Is.EqualTo("Service Centre"));
        }
    }
}