using System;
using System.Threading.Tasks;

namespace SalesApp.Core.Tests.BL.Commissions
{
    public class DateServiceTest
    {
        // [TestFixture] Todo Fix below issues in each test
        public class CustomersControllerTest
        {
            // [Test] Todo System.FormatException : String was not recognized as a valid DateTime.
            public async Task SaveCustomerTest()
            {
                DateTime date = DateTime.Parse("January");

                Console.WriteLine(date.Day);
            }
        }
    }
}