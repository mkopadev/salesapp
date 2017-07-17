using System;
using NUnit.Framework;

namespace SalesApp.Core.Tests.Services.Locations
{
    [TestFixture]
    public class LocationServiceTest : TestsBase
    {
        private string appVersion = "1.6";
        private string smsShortCode = "20232";
        private string location;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            this.location = "1.3609+36.0983";
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            // await this.localOtaService.Db.Connection.DeleteAllAsync<OtaSetting>();
            this.Logger.Debug("Deleted all all location settings");
        }

        [Test]
        public void TestLocationInApiCall()
        {
            Console.WriteLine("Testing location");
            //this.Logger.Debug("Testing Memory " + Cache<Location>.Get<string>().Get("location"));
        }

        public void AddLocationToApiCall()
        {
            
        }
    }
}
