using System;
using NUnit.Framework;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.Tests.Extensions
{
    [TestFixture]
    public class DateExtensionsTest : TestsBase
    {
        [Test]
        public void ToMidnightTest()
        {
            DateTime dt = new DateTime(2016, 4, 16, 8, 6, 30, 122);
            dt = dt.ToMidnight();
            Assert.That(dt.Millisecond, Is.EqualTo(0));
            Assert.That(dt.Second, Is.EqualTo(0));
            Assert.That(dt.Minute, Is.EqualTo(0));
            Assert.That(dt.Hour, Is.EqualTo(0));
            Assert.That(dt.Day, Is.EqualTo(16));
            Assert.That(dt.Month, Is.EqualTo(4));
            Assert.That(dt.Year, Is.EqualTo(2016));
        }

        [Test]
        public void ToEndOfDayTest()
        {
            DateTime dt = new DateTime(2016, 4, 16, 8, 6, 30, 122);
            dt = dt.ToEndOfDay();
            Assert.That(dt.Millisecond, Is.EqualTo(999));
            Assert.That(dt.Second, Is.EqualTo(59));
            Assert.That(dt.Minute, Is.EqualTo(59));
            Assert.That(dt.Hour, Is.EqualTo(23));
            Assert.That(dt.Day, Is.EqualTo(16));
            Assert.That(dt.Month, Is.EqualTo(4));
            Assert.That(dt.Year, Is.EqualTo(2016));
        }
    }
}
