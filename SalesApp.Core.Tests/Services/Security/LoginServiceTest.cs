using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mkopa.Core.Services.Security;
using NUnit.Framework;

namespace MKopa.CoreTests.Services.Security
{
    [TestFixture]
    public class LoginServiceTest : BaseTest
    {
        [Test]
        public void LoginValidTest()
        {
            LoginService service = new LoginService();
            Assert.True(service.LoginValid(new DateTime(2015, 8, 2, 3, 0, 0), new DateTime(2015, 7, 23, 3, 0, 0), 10));
            Assert.False(service.LoginValid(new DateTime(2015, 8, 2, 3, 0, 0), new DateTime(2015, 7, 22, 3, 0, 0), 10));

            Assert.True(service.LoginValid(new DateTime(2015, 8, 2, 3, 0, 0), new DateTime(2015, 7, 23, 3, 0, 0), 11));
            Assert.True(service.LoginValid(new DateTime(2015, 8, 2, 3, 0, 0), new DateTime(2015, 7, 22, 3, 0, 0), 11));

            Assert.False(service.LoginValid(new DateTime(2015, 8, 2, 3, 0, 0), new DateTime(2015, 7, 23, 3, 0, 0), 9));
            Assert.False(service.LoginValid(new DateTime(2015, 8, 2, 3, 0, 0), new DateTime(2015, 7, 22, 3, 0, 0), 9));
        }
    }
}
