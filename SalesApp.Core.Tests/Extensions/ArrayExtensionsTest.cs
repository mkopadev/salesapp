using NUnit.Framework;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.Tests.Extensions
{
    [TestFixture]
    public class ArrayExtensionsTest
    {
        [Test]
        public void SelectNextItemTest()
        {
            string[] arr = { "apple", "peer", "orange" };
            Assert.That(arr.SelectNextItem("apple"), Is.EqualTo("peer"));
            Assert.That(arr.SelectNextItem("peer"), Is.EqualTo("orange"));
            Assert.That(arr.SelectNextItem("orange"), Is.Null);
            Assert.That(arr.SelectNextItem("pumpkin"), Is.Null);
            Assert.That(arr.SelectNextItem(null), Is.Null);
        }

        [Test]
        public void SelectPreviousItemTest()
        {
            string[] arr = { "apple", "peer", "orange" };
            Assert.That(arr.SelectPreviousItem("apple"), Is.Null);
            Assert.That(arr.SelectPreviousItem("peer"), Is.EqualTo("apple"));
            Assert.That(arr.SelectPreviousItem("orange"), Is.EqualTo("peer"));
            Assert.That(arr.SelectPreviousItem("pumpkin"), Is.Null);
            Assert.That(arr.SelectPreviousItem(null), Is.Null);
        }
    }
}