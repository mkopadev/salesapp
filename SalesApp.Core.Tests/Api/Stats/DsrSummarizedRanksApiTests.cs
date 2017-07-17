using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Tests.Api.Stats
{
    [TestFixture]
    public class DsrSummarizedRanksApiTests : ApiTestsBaseClass<DsrRankingSummarizedApi, RankingSummarized>
    {
        private string GetRawResponseString(List<RankingSummarized> obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        [Test]
        public async override Task ApiCallsOnline()
        {
            List<RankingSummarized> responseList = new List<RankingSummarized>
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
            responseList = responseList.OrderBy(item => item.Level).ToList();
            DefineResponse
                (
                    new ServerResponse<List<RankingSummarized>>
                    {
                        IsSuccessStatus = true
                        ,RawResponse = GetRawResponseString
                        (
                           responseList
                        )
                        ,StatusCode = HttpStatusCode.OK
                    }
                );
            ServerResponse<List<RankingSummarized>> serverResponse = await StandardFetch(default(CancellationToken));
            Assert.AreEqual((double)HttpStatusCode.OK,(double)serverResponse.StatusCode,"Invalid server response code");
            List<RankingSummarized> serverList = serverResponse.GetObject().OrderBy(item => item.Level).ToList();
            Assert.AreEqual((double)responseList.Count,(double)serverList.Count,"Wrong number of results returned");
            for (int i = 0; i < serverList.Count; i++)
            {
                Assert.IsTrue(responseList[i].Area == serverList[i].Area,"Area name returned by server does not match with area provided");
            }
        }

        [Test]
        public async override Task ApiCallsOffline()
        {
            DefineResponse(new NotConnectedToInternetException());
            ServerResponse<List<RankingSummarized>> serverResponse = null;
            Assert.Throws<NotConnectedToInternetException>
                (
                    async () =>
                    {
                        serverResponse = await StandardFetch(default(CancellationToken));
                    }
                );
            Assert.IsNull(serverResponse);
        }

        private async Task<ServerResponse<List<RankingSummarized>>> StandardFetch(CancellationToken cancellationToken)
        {
            return await Api.MakeGetCallAsync<List<RankingSummarized>>(Resolver.Instance.Get<ISalesAppSession>().UserId.ToString());
        }

        public override Task ApiCallsCanceled()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async override Task MockResponse400()
        {
            DefineResponse
                (
                    new ServerResponse<List<RankingSummarized>>
                    {
                        IsSuccessStatus = false
                        ,RawResponse = "{}"
                        ,StatusCode = HttpStatusCode.NotFound
                    }
                );
            var serverResponse = default(ServerResponse<List<RankingSummarized>>);
            Assert.DoesNotThrow
                (
                    async () =>
                    {
                        serverResponse = await StandardFetch(default(CancellationToken));
                    }
                );
            Assert.IsTrue(HttpStatusCode.NotFound == serverResponse.StatusCode);
            
        }

        public override Task MockResponse401()
        {
            throw new NotImplementedException();
        }

        public override Task MockResponse404()
        {
            throw new NotImplementedException();
        }

        public override Task MockResponse500()
        {
            throw new NotImplementedException();
        }

        public override Task MockResponse503()
        {
            throw new NotImplementedException();
        }

        public override Task MockNullResponse()
        {
            throw new NotImplementedException();
        }

        public override Task MockMalformedJsonResponse()
        {
            throw new NotImplementedException();
        }

        public override Task MockWrongJsonResponse()
        {
            throw new NotImplementedException();
        }

        public override Task MockTimeout()
        {
            throw new NotImplementedException();
        }
    }
}