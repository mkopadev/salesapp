using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Services.Person.Json;

namespace SalesApp.Core.Tests.Api
{
    [TestFixture]
    public class UserDtoSerializer
    {
        private string _userDtoJson = "{\"FirstName\":\"Barbara\",\"LastName\":\"Wanjohi\",\"Hash\":\"ce04df06b6653ae4a4591f2bee58db1661ec21448be0fa5f44735536d3f8cbca\",\"Id\":\"61c466f3-6f5f-e411-80d8-00155d83e77c\"}";

        [Test]
        public void TestSerialize()
        {
            UserDto userDto = new Serializer().Deserialize<UserDto>(_userDtoJson);
            Assert.That(userDto.FirstName, Is.EqualTo("Barbara"));
        }
    }
}
