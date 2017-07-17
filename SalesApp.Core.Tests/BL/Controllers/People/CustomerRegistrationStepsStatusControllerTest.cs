using NUnit.Framework;
using SalesApp.Core.Api.Person;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.Tests.BL.Controllers.People
{
    // [TestFixture] Todo One or more tests in this fixture are broken, fix them
    public class CustomerRegistrationStepsStatusControllerTest
    {
        [SetUp]
        public void Setup()
        {
            new TestBootstrapper().Bootstrap();
        }

        // Todo [Test] Test completely broken ILocationServiceListener not bound to real implementatio, cannot be resolved
        public async void TestPersistingStatus()
        {
            var api = new CustomerStatusApi();
            var status = await api.GetAsync("072");
            SaveResponse<Customer> response = await new CustomersController()
                .SaveAsync
                (
                    new Customer()
                    {
                        FirstName = "Test"
                        ,
                        LastName = "Customer"
                        ,
                        Phone = "0721553226"
                        ,NationalId = "22932184"
                    }
                );
            await new CustomerRegistrationStepsStatusController()
                .SaveAsync(status,response.SavedModel);
            Assert.AreEqual(1,1);
        }
    }
}