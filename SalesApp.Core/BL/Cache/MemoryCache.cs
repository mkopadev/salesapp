using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using SalesApp.Core.Logging;
using SalesApp.Core.Services;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.BL.Cache
{
    public class MemoryCache : ICache
    {
        public readonly static string CacheName = "SalesApp";

        private readonly ILog logger = Resolver.Instance.Get<ILog>();
        private readonly ITimeService timeService = TimeService.Get();

        private static readonly Lazy<MemoryCache> INSTANCE = new Lazy<MemoryCache>(() => new MemoryCache());

        private IBlobCache blobInMemoryCache = null;
        private IBlobCache blobDbCache = null;

        private CacheType CacheType { get; set; }

        public MemoryCache SetCachingType(CacheType type)
        {
            this.CacheType = type;
            return this;
        }

        public static MemoryCache Instance
        {
            get { return INSTANCE.Value; }
        }

        private MemoryCache()
        {
            BlobCache.ApplicationName = CacheName;
            CleanExpiredcache();
        }

        public async Task Store<T>(string key, T obj,  CacheType type = CacheType.INMEMORY, int timeoutMinutes = 1440)
        {
            try
            {
                var expiryDateTime = this.timeService.Now.AddMinutes(timeoutMinutes);
                await this.RemoveObject(key, type);
                if (type == CacheType.DB)
                {
                    await BlobCache.LocalMachine.InsertObject(key, obj, expiryDateTime);
                }
                else
                {
                    // set the Expiry date to now + defaulttimeout
                    await BlobCache.InMemory.InsertObject(key, obj, expiryDateTime);
                }
            }
            catch (Exception ke)
            {
                this.logger.Debug(ke);
            }
        }

        public async Task<T> Get<T>(string key, CacheType type = CacheType.INMEMORY)
        {
            try
            {
                T t = default(T);

                if (type == CacheType.DB)
                {
                    t = await BlobCache.LocalMachine.GetObject<T>(key);
                }
                else
                {
                    t = await BlobCache.InMemory.GetObject<T>(key);
                }

                return t;
            }
            catch (Exception ke)
            {
                this.logger.Verbose(ke.Message);
                return default(T);
            }
        }

        public async Task RemoveObject(string key,CacheType type = CacheType.INMEMORY)
        {
            try
            {
                if (type == CacheType.DB)
                {
                    await BlobCache.LocalMachine.Invalidate(key);
                }
                else
                {
                    await BlobCache.InMemory.Invalidate(key);
                }
            }
            catch (Exception e)
            {
                this.logger.Verbose(e.Message);
            }
        }

        public async Task CleanExpiredcache()
        {
            try
            {
                await BlobCache.LocalMachine.Vacuum();
            }
            catch (Exception ke)
            {
                this.logger.Verbose(ke.Message);
            }
        }
    }
}