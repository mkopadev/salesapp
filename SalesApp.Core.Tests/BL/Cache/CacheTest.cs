using System;
using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.BL.Cache;
using SalesApp.Core.Services;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Core.Tests.BL.Cache
{
    // [TestFixture] Todo: One or more tests in the fixture is broken
    public class CacheTest // : TestsBase
    {
        // [Test] Todo Test is broken
        public void TestCache()
        {
            ITimeService mockTimeService = Substitute.For<ITimeService>();
            mockTimeService.Now.Returns(new DateTime(2015, 10, 22, 9, 0, 0));

            Settings mockLegacySettings = Substitute.For<Settings>();
            mockLegacySettings.DefaultCacheTimeout.Returns(5);

            MemoryCache cache = MemoryCache.Instance;

            cache.Store("test-key", new ObjectToCache { Name = "cached-object-name" });

            // set time 4 minutes later, cache still valid
            mockTimeService.Now.Returns(new DateTime(2015, 10, 22, 9, 4, 0));
            var result = cache.Get<ObjectToCache>("test-key");
            Assert.That(result, Is.Not.Null);

            // non existing key
            result = cache.Get<ObjectToCache>("non-existing-key");
            Assert.That(result, Is.Null);

            // set time 5 minutes later, cache still valid
            mockTimeService.Now.Returns(new DateTime(2015, 10, 22, 9, 5, 0));
            result = cache.Get<ObjectToCache>("test-key");
            Assert.That(result, Is.Not.Null);

            // set time 6 minutes later, cache is not valid anymore
            mockTimeService.Now.Returns(new DateTime(2015, 10, 22, 9, 6, 0));
            result = cache.Get<ObjectToCache>("test-key");
            Assert.That(result, Is.Null);

            // set time back to 4 minutes later, it should be removed from the cache
            mockTimeService.Now.Returns(new DateTime(2015, 10, 22, 9, 4, 0));
            result = cache.Get <ObjectToCache>("test-key");
            Assert.That(result, Is.Null);
        }

        private class ObjectToCache
        {
            /// <summary>
            /// Name of the object.
            /// </summary>
            public string Name { get; set; }
        }
    }
}
