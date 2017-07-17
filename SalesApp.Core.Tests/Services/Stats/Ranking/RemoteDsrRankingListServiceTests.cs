using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Services.Stats.Ranking;

namespace SalesApp.Core.Tests.Services.Stats.Ranking
{
    [TestFixture]
    public class RemoteDsrRankingListServiceTests : RemoteServicesTestsBase<DsrRankingListApi, DsrRankingList, RemoteDsrRankingListService, DsrRankInfo>
    {
        protected override DsrRankingList ValidResponse
        {
            get
            {
                return new DsrRankingList
                {
                    TimeStamp = DateTime.Now
                    ,
                    Dsrs = new List<DsrRankInfo>
                    {
                        new DsrRankInfo
                        {
                            Name = "John Peter"
                            ,
                            Rank = 17
                            ,
                            Sales = 100
                            ,
                            IsMe = false
                        }
                        ,
                        new DsrRankInfo
                        {
                            IsMe = true
                            ,
                            Rank = 18
                            ,
                            Name = "John Irungu"
                            ,
                            Sales = 99
                        }
                    }
                };
            }
        }

        [Test]
        public override async Task ApiCallsOnline()
        {
        }

        public override Task ApiCallsOffline()
        {
            throw new NotImplementedException();
        }

        public override Task ApiCallsCanceled()
        {
            throw new NotImplementedException();
        }

        public override Task MockCommonHttpFailureCodes()
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

        public override Task ActualOnlineCall()
        {
            throw new NotImplementedException();
        }
    }
}