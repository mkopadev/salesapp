using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.BL.Models.Stats.Sales;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Services.Stats.Sales;

namespace SalesApp.Core.Tests.Services.Stats.Sales
{
    [TestFixture]
    public class RemoteSalesStatsServiceTests : RemoteServicesTestsBase<SalesStatsApi, StatHeader,RemoteSalesStatsService,Stat>
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void TestDaysBelongsToCorrectWeek()
        {
            string dateFormat = "dd-MMM-yyyy";
            Settings.Instance.StartOfWeek = DayOfWeek.Sunday;
            DayOfWeek startofDayOfWeek = Settings.Instance.StartOfWeek;

            LocalSalesStatsService salesStatsService = new LocalSalesStatsService();
            DateTime dateToTest = new DateTime
                (
                    2015, 08, 1
                );

            DateTime startOfWeek = new DateTime(2015, 07, 26);

            for (int i = 0; i < 365; i++)
            {
                Logger.Debug("Testing ~".GetFormated(dateToTest.ToLongDateString()));
                Tuple<DateTime, DateTime> range = salesStatsService.GetWeekDayBelongsTo(dateToTest);
                DateTime start = range.Item1;
                DateTime end = range.Item2;
                Logger.Debug("Week starts on ~".GetFormated(start.ToLongDateString()));
                Logger.Debug("Week ends on ~".GetFormated(end.ToLongDateString()));
                Assert.IsTrue
                    (
                        startOfWeek.ToString(dateFormat) == start.ToString(dateFormat)
                        , "Expected start of week to be ~ but instead it was ~"
                        .GetFormated(startOfWeek.ToString(dateFormat), start.ToString(dateFormat))
                    );
                dateToTest = dateToTest.AddDays(1);
                if (dateToTest.DayOfWeek == startofDayOfWeek)
                {
                    Logger.Debug("Start of week has now been changed to ~".GetFormated(dateToTest.ToLongDateString()));
                    startOfWeek = dateToTest;
                }
            }
        }

        [Test]
        public async Task GetSalesTodayAsyncNeverNegative()
        {
            LocalSalesStatsService salesStatsService = new LocalSalesStatsService();
            int res = await salesStatsService.GetSalesTodayAsync();
            Logger.Debug("Total sales today was ~".GetFormated(res));
            Assert.Greater(res, -1, "How did you achieve negative sales? I should be interested to know sir.");
        }

        // [Test] Broken test
        public async override Task ApiCallsOnline()
        {
            /*int prospectsCount = 26125;
            int salesCount = 265487;
            DefineResponse
                (
                    new ServerResponse<StatHeader>
                    {
                        IsSuccessStatus = true
                        ,
                        StatusCode = HttpStatusCode.OK
                        ,
                        RawResponse = JsonConvert.SerializeObject
                        (
                            new AggregatedStat
                            {
                                Period = Period.Day
                                ,
                                Sales = salesCount
                                ,
                                Prospects = prospectsCount
                                ,
                                From = DateTime.Now
                                ,
                                To = DateTime.Now
                            }
                        )

                    }
                );
           
            Assert.DoesNotThrow
                (
                    async () => await RemoteService.UpdateStats()
                );*/
        }

        protected override StatHeader ValidResponse
        {
            get { throw new NotImplementedException(); }
        }

        // [Test] Broken test
        public async override Task ApiCallsOffline()
        {
            /*DefineResponse(new NotConnectedToInternetException());
            Assert.Throws<NotConnectedToInternetException>
                (
                    async () => await RemoteService.UpdateStats()
                );*/
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

        [Test]
        public override async Task ActualOnlineCall()
        {
            RemoteSalesStatsService service = new RemoteSalesStatsService();

            Assert.DoesNotThrow(async () => await service.UpdateStats(Period.Day));
        }
    }
}