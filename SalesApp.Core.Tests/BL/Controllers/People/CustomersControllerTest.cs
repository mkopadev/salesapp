using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.Tests.BL.Controllers.People
{
    // [TestFixture] Todo One or more tests in this fixture are broken, fix them
    public class CustomersControllerTest
    {
        [SetUp]
        public void Setup()
        {
            try
            {
                new TestBootstrapper().Bootstrap();
            }
            catch (Exception exception)
            {
                exception.StackTrace.WriteLine();
                throw;
            }
        }

        // Todo [Test] Test completely broken ILocationServiceListener not bound to real implementatio, cannot be resolved
        public async Task SaveCustomerTest()
        {
            Customer customer = new Customer
            {
                Id = Guid.NewGuid()
                ,
                Channel = DataChannel.Fallback
                ,
                DsrPhone = "0721553229"
                ,
                FirstName = "Jake"
                ,
                LastName = "Roberts"
                ,
                NationalId = "22932568"
                ,
                Phone = "0722456789"
                ,
                UserId = Guid.NewGuid()
            };
            SaveResponse<Customer> saveResponse = await new CustomersController().SaveAsync(customer);
            SyncRecord syncRecord = await new SyncingController().GetSyncRecordAsync(saveResponse.SavedModel.RequestId);
            Assert.NotNull(syncRecord,"No sync record found");
        }
    }
}