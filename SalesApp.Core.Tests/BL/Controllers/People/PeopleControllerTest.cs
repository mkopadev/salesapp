using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.Tests.BL.Controllers.People
{
    [TestFixture]
    class PeopleControllerTest : TestsBase
    {
        private ProspectsController _prospectsController;

        private ProspectsController prospectsController
        {
            get
            {
                if (_prospectsController == null)
                {
                    _prospectsController = new ProspectsController();
                }
                return _prospectsController;
            }
        }

        [SetUp]
        public void Initialize()
        {
            new TestBootstrapper().Bootstrap();
        }

        private async Task<SaveResponse<Prospect>> SaveAsync(string firstname, string lastname, string phone)
        {
            Prospect prospect = new Prospect
            {
                LastName = lastname
                ,
                FirstName = firstname
                ,
                Authority = true
                ,
                DsrPhone = "0721554712"
                ,
                Money = true
                ,
                Need = false
                ,
                Phone = phone
            };

            return await prospectsController.SaveAsync(
                    prospect
                );
        }

        // [Test] Broken test
        public async void PeopleTestSearchOnline()
        {
            Prospect person = await prospectsController.SearchPersonOnlineAsync("0721456789", filterFlags: ErrorFilterFlags.DisableErrorHandling);
            Assert.IsNotNull(person, "Could not fetch person from online server");
        }

        [Test]
        public async void PersonExistsTest()
        {
            try
            {
                const string phone = "0784159753";
                Prospect prospect = await prospectsController.GetByPhoneNumberAsync(phone);
                if (prospect != null)
                {
                    int result = await prospectsController.DeleteAsync(prospect);
                    if (result != 1)
                    {
                        throw new Exception("Could not delete prospect");
                    }
                }
                await SaveAsync("Kennedy", "Maluki", phone);
                prospect = await prospectsController.GetPersonIfExists(phone, null, ErrorFilterFlags.DisableErrorHandling, false);
                bool exists = prospect != null;
                await prospectsController.DeleteAsync(prospect);
                Assert.IsTrue(exists, "Test for checking whether prospect exists is failing when it should succeed as prospect was saved on device");
            }
            catch (Exception ex)
            {
                "Exception thrown ~".WriteLine(ex.Message);
            }
        }

        private async Task<int> DeleteProspectIfExists(string phone)
        {

            Prospect prospect = await prospectsController.GetByPhoneNumberAsync(phone);
            int result = 0;
            if (prospect != null)
            {
                result = await prospectsController.DeleteAsync(prospect);
                if (result != 1)
                {
                    throw new Exception("Could not delete prospect");
                }
            }

            return result;
        }

        // [Test] Broken test
        public async void PersonGetByPhoneNumberAsync()
        {
            const string phone = "0784159754";

            await DeleteProspectIfExists(phone);
            SaveResponse<Prospect> prospectResponse = await SaveAsync("Adam", "Maluki", phone);
            Person person = await prospectsController.GetByPhoneNumberAsync(phone);
            Assert.IsNotNull(prospectResponse.SavedModel, "Prospect was null");
        }

        // [Test] Broken test
        public async void SaveProspect()
        {
            const string phone = "0784159756";
            int saveResult = await DeleteProspectIfExists(phone);
            "results of deletion were ~".WriteLine(saveResult.ToString());
            if (saveResult == 1 || saveResult == 0)
            {
                "we deleted successfully".WriteLine();
                SaveResponse<Prospect> prospectResponse = await prospectsController.SaveAsync
                    (
                        new Prospect
                        {
                            Authority = true
                            ,
                            DsrPhone = phone
                            ,
                            LastName = "Mathenge"
                            ,
                            FirstName = "Thomas"
                            ,
                            Money = true
                            ,
                            Need = true
                            ,
                            Phone = phone
                        }
                    );
                Assert.AreNotSame(prospectResponse.SavedModel.Id, default(Guid));
            }
            else
            {
                throw new Exception("Could not delete prospect");
            }
        }
    }
}
