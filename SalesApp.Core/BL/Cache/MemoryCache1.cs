using SalesApp.Core.Logging;

namespace SalesApp.Core.BL.Cache
{
    public class MemoryCache1
    {
        private static readonly ILog Logger = LogManager.Get(typeof(MemoryCache1));

         public static MemoryCache1 SingletonInstance = new MemoryCache1();

        private MemoryCache1()
        {
        }

        public MemoryCache Get()
        {
            return MemoryCache.Instance;
        }

        //////////////////////////////
        /// 
        /// 
        /// 
        /// 
    }
}
