using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.Api.Json;

namespace SalesApp.Core.Tests.Api.Json
{
    [TestFixture]
    public class TolerantConverterTest
    {
        [Test]
        public void ConvertEnumTest()
        {
            string json1 = "{Id : \"12\", EnumTest : \"Value2\", EnumTest2 : \"Value22\"}";
            string json2 = "{Id : \"22\", EnumTest : \"Value3\", EnumTest2 : \"Value33\"}";

            var result = JsonConvert.DeserializeObject<ToConvert>(json1);
            Assert.That(result.Id, Is.EqualTo("12"));
            Assert.That(result.EnumTest, Is.EqualTo(EnumTest.Value2));
            Assert.That(result.EnumTest2, Is.EqualTo(EnumTest2.Value22));

            result = JsonConvert.DeserializeObject<ToConvert>(json2);
            Assert.That(result.Id, Is.EqualTo("22"));
            Assert.That(result.EnumTest, Is.EqualTo(EnumTest.Value1));
            Assert.That(result.EnumTest2, Is.EqualTo(EnumTest2.Unknown));
        }

        [JsonConverter(typeof(TolerantEnumConverter))]
        public enum EnumTest
        {
            Value1,
            Value2
        }

        [JsonConverter(typeof(TolerantEnumConverter))]
        public enum EnumTest2
        {
            Value12,
            Value22,
            Unknown
        }

        public class ToConvert
        {
            public string Id { get; set; }

            public EnumTest EnumTest { get; set; }

            public EnumTest2 EnumTest2 { get; set; }
        }
    }
}