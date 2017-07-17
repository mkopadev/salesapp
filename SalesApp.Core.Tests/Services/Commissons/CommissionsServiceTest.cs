using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.Api.Commissions;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.Commissions;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Locations;
using SalesApp.Core.Tests.Services.Locations;

namespace SalesApp.Core.Tests.Services.Commissons
{
    [TestFixture]
    public class CommissionsServiceTest : TestsBase
    {
        private CommissionService _service;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            object[] constArgs = { "commissions" };

            var api = Substitute.For<CommissionApi>(constArgs);

            this._service = new CommissionService(api);
        }

        [Test]
        public void TestGetCommissionSummaryWithNoInternetThorwsNotConnectedToInternetException()
        {
            var locationListener = Substitute.For<LocationServiceListener>();
            var connectivityService = Substitute.For<IConnectivityService>();
            connectivityService.HasConnection().Returns(false);

            Resolver.Instance.RegisterSingleton<ILocationServiceListener>(locationListener);
            Resolver.Instance.RegisterSingleton<IConnectivityService>(connectivityService);

            Assert.Throws<NotConnectedToInternetException>(
            async () => await this._service.GetCommissionSummary("/summary", "CacheKey"));
         }
     }
}
