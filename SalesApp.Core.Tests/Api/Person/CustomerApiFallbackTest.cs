using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.Api.Person;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Connectivity;

namespace SalesApp.Core.Tests.Api.Person
{
    [TestFixture]
    public class CustomerApiFallbackTest : TestsBase
    {
        private Customer _testCustomer
        {
            get
            {
                return new Customer
                {
                    Id = ApiGuid,
                    RequestId = ApiGuid,
                    UserId = LocalGuid,
                    Product =
                        new Product
                        {
                            DisplayName = "DisplayName",
                            ProductTypeId = ApiGuid,
                            Id = LocalGuid,
                            SerialNumber = "serial"
                        },
                    FirstName = "John",
                    LastName = "Doe",
                    Phone = "01234456789",
                    NationalId = "123123",
                    DsrPhone = "0712345678"
                };
            }
        }

        [Test]
        public async void Test()
        {
            ISmsService mockService = Substitute.For<ISmsService>();
            mockService.SendSms(Arg.Any<string>(), Arg.Any<string>())
                .Returns(true, false);

            CustomerApiFallback fallback = new CustomerApiFallback(mockService);
            var result = await fallback.RegisterCustomer(this._testCustomer);
            Assert.That(result, Is.True);
            result = await fallback.RegisterCustomer(this._testCustomer);
            Assert.That(result, Is.False);
        }
    }
}
