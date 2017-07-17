using System.Net;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Api.OtaSettings;
using SalesApp.Core.BL.Models.OtaSettings;
using SalesApp.Core.Services.OtaSettings;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Core.Tests.Services.OtaSettings
{
    [TestFixture]
    public class OtaSettingsTest : TestsBase
    {
        private LocalOtaService localOtaService;

        private string requestParams;
        private string serverTimestamp = "2015-09-21T00:00";
        private string appVersion = "1.6";
        private string smsShortCode = "20232";

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.requestParams = string.Format("?servertimestamp={0}&appversion={1}", this.serverTimestamp, this.appVersion);
            this.Logger.Debug("Setting up test.");
            this.localOtaService = new LocalOtaService();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            // await this.localOtaService.Db.Connection.DeleteAllAsync<OtaSetting>();
            this.Logger.Debug("Deleted all OtaSettings. Next test");
            this.localOtaService = null;
        }

        // [Test] Failing test, please fix it
        public async void TestFetchOtaSettings_Success()
        {
            string productsArray = @"[{'Id':'16732CB0-B418-E411-9439-000C29921969','Name':'Product x'},{'Id':'5D5F4B4F-6CDA-E311-9411-000C29B398FA','Name':'Product III'}]".Replace("'", "\"");

            string responseJson = @"
                {
                 'AppVersion': '1.6',
                 'ServerTimeStamp': '2015-09-21T00:00',
                 'ConfigGroups' : [
                 {
                    'Name' : 'Registration',
                    'Value': [
                        {
                            'Name' : 'SMSShortCode',
                            'Value':  20232
                        },
                        {
                            'Name': 'Products',
                            'Value': [
                                {
                                    'Id': '16732CB0-B418-E411-9439-000C29921969',
                                    'Name': 'Product X'
                                },
                                {
                                    'Id': '5D5F4B4F-6CDA-E311-9411-000C29B398FA',
                                    'Name': 'ProductA III'
                                }
                            ]
                        }
                    ]
                 },
                 {
                    'Name' : 'Support',
                    'Value' : [
                        {
                            'Name' : 'DsrWizard',
                            'Value' : 'string'
                        },
                        {
                            'Name' : 'CustomerWizard',
                            'Value' : {'Id' : 'Blah', 'Name' : 'Blah blah'}
                        }
                    ]
                 }
                 ]
                }";
            await this.MakeApiCall(responseJson, true, HttpStatusCode.OK);

            string dbSmsShortCode = this.localOtaService.GetString(LocalOtaService.Registration, "SMSShortCode");
            string dbServerTimeStamp = this.localOtaService.GetString(LocalOtaService.Communication, "ServerTimeStamp");
            string dbProducts = this.localOtaService.GetString(LocalOtaService.Registration, "Products");

            Assert.That(dbServerTimeStamp, Is.EqualTo(this.serverTimestamp));
            Assert.That(dbSmsShortCode, Is.EqualTo(this.smsShortCode));
            Assert.That(dbProducts, Is.EqualTo(productsArray));

            Settings settings = Settings.Instance;
            Assert.That(settings.ServerTimeStamp, Is.EqualTo(this.serverTimestamp));
            Assert.That(settings.Products, Is.EqualTo(dbProducts));
            Assert.That(settings.SMSShortCode, Is.EqualTo(this.smsShortCode));

            string dsrWizard = this.localOtaService.GetString(LocalOtaService.Support, "DsrWizard");
            Assert.That(dsrWizard, Is.EqualTo("string"));
            string dbCustomerWizard = this.localOtaService.GetString(LocalOtaService.Support, "CustomerWizard");

            string customerWizard = @"{'Id':'Blah','Name':'Blah blah'}".Replace("'", "\"");

            Assert.That(dbCustomerWizard, Is.EqualTo(customerWizard));

            // hit api again and make sure values are updating
            responseJson = @"
                {
                 'AppVersion': '1.6',
                 'ServerTimeStamp': '2015-09-21T00:00',
                 'ConfigGroups' : [
                 {
                    'Name' : 'Registration',
                    'Value': [
                        {
                            'Name' : 'SMSShortCode',
                            'Value':  20233
                        },
                        {
                            'Name': 'Products',
                            'Value': [
                                {
                                    'Id': '16732CB0-B418-E411-9439-000C29921969',
                                    'Name': 'Product X'
                                },
                                {
                                    'Id': '5D5F4B4F-6CDA-E311-9411-000C29B398FA',
                                    'Name': 'Product III'
                                }
                            ]
                        }
                    ]
                 },
                 {
                    'Name' : 'Support',
                    'Value' : [
                        {
                            'Name' : 'DsrWizard',
                            'Value' : 'string'
                        },
                        {
                            'Name' : 'CustomerWizard',
                            'Value' : {'Id' : 'Blah', 'Name' : 'Blah blah'}
                        }
                    ]
                 }
                 ]
                }";

            await this.MakeApiCall(responseJson, true, HttpStatusCode.OK);
            dbSmsShortCode = this.localOtaService.GetString(LocalOtaService.Registration, "SMSShortCode");

            // make sure we update any changed vaues
            Assert.That("20233", Is.EqualTo(dbSmsShortCode));
        }

        [Test]
        public async Task TestUnknownSettingsAreIgnored()
        {
            const string responseJson = @"
                {
                 'AppVersion': '1.6',
                 'ServerTimeStamp': '2015-09-21T00:00',
                 'ConfigGroups' : [
                 {
                    'Name' : 'Support',
                    'Value' : [
                        {
                            'Name' : 'UnknownSetting',
                            'Value' : 'Some Value'
                        }
                    ]
                 }
                 ]
                }";
            await this.MakeApiCall(responseJson, true, HttpStatusCode.OK);

            // check that UnknownSetting was not saved to database
            string dbValue = this.localOtaService.GetString(LocalOtaService.Support, "Unknown");

            Assert.That(null, Is.EqualTo(dbValue));
        }

        [Test]
        public async Task TestCommonApiFailureCases()
        {
            string response = string.Empty;
            await this.MakeApiCall(response, false, HttpStatusCode.InternalServerError);
            await this.MakeApiCall(response, false, HttpStatusCode.NoContent);
            await this.MakeApiCall(response, false, HttpStatusCode.BadRequest);
        }

        private async Task MakeApiCall(string rawResponse, bool status, HttpStatusCode httpStatusCode)
        {
            var serverResponse = Task.FromResult(new ServerResponse<OtaServerResponse> { RawResponse = rawResponse, IsSuccessStatus = status, StatusCode = httpStatusCode });
            var api = Substitute.For<OtaSettingsApi>();
            api.MakeGetCallAsync<OtaServerResponse>(this.requestParams).Returns(serverResponse);

            RemoteOtaService otaService = new RemoteOtaService(api, this.localOtaService);
            await otaService.FetchOtaSettingsAsync(this.requestParams);
        }
    }
}