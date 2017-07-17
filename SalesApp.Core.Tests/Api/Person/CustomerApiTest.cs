using System;

namespace SalesApp.Core.Tests.Api.Person
{
    // [TestFixture] Todo one or more tests in the fixture are broken
    public class CustomerApiTest // : TestsBase
    {
        private Guid _guid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // [Test] Todo Test completely broken ILocationService not bound to real implementatio, cannot be resolved
        /*public async void SendCustomer201()
        {
            Customer customer = new Customer
            {
                Id = ApiGuid,
                RequestId = ApiGuid,
                UserId = _guid,
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = default(Guid), Id = _guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };
            CustomerApi customerApi = new CustomerApi();
            customerApi.BaseUrl = MockApi;

            var result = await customerApi.RegisterCustomer(customer, ApiTimeoutEnum.Normal); 

            Assert.That(result.Customer.Id, Is.EqualTo(ApiGuid));
            Assert.That(result.RequestId, Is.EqualTo(ApiGuid));
            Assert.That(result.ResponseText, Is.EqualTo("Success"));
            Assert.That(result.Successful, Is.True);
        }*/

        // [Test] Todo Test completely broken ILocationService not bound to real implementatio, cannot be resolved
        /*public async void SendCustomer200()
        {
            Customer customer = new Customer
            {
                Id = ApiGuid,
                RequestId = ApiGuid,
                UserId = _guid,
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = default(Guid), Id = _guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };

            CustomerApi customerApi = new CustomerApi();
            customerApi.BaseUrl = MockApi;

            customerApi.AddHeader("Preferred", "status=200");

            var result = await customerApi.RegisterCustomer(customer, ApiTimeoutEnum.Normal);

            Assert.That(result.ResponseText, Is.EqualTo("Customer already exists."));
            Assert.That(result.Successful, Is.False);
            Assert.That(result.RequestId, Is.EqualTo(ApiGuid));
        }*/
    }
}
