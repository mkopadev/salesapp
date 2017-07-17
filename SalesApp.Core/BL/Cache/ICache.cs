using System.Threading.Tasks;

namespace SalesApp.Core.BL.Cache
{
    public interface ICache
    {
        /// <summary>
        /// Gets an object of cache
        /// </summary>
        /// <param name="key">Unique identifier for the object the memory</param>
        /// <param name="type">Location of object in cache ie memory or sqlite db</param>
        /// <returns>T obj. cached object</returns>
        Task<T> Get<T>(string key, CacheType type = CacheType.INMEMORY);

        Task Store<T>(string key, T obj, CacheType type = CacheType.INMEMORY, int defaultCacheTimeout = 1440);

        /// <summary>
        /// Removes an object from cache
        /// </summary>
        /// <param name="key">Unique identifier for the object the memory</param>
        /// <param name="type">Location of object in cache ie memory or sqlite db</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task RemoveObject(string key, CacheType type = CacheType.INMEMORY);

        /// <summary>
        /// Removes expired objects from the cache
        /// </summary>
        Task CleanExpiredcache();
    }
}
