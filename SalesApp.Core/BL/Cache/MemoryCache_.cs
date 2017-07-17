using System;
using System.Collections.Generic;
using Mkopa.Core.Extensions;
using Mkopa.Core.Logging;

namespace Mkopa.Core.BL.Cache
{
    public class MemoryCache_
    {
        private static readonly ILog Logger = LogManager.Get(typeof(MemoryCache_));

         public static MemoryCache_ SingletonInstance = new MemoryCache_();
        // private static readonly Dictionary<string, object> Cache = new Dictionary<string, object>();

        private MemoryCache_()
        {
        }

        public MemoryCache<T> Get<T>()
        {
            string name = typeof(T).AssemblyQualifiedName;

            Logger.Debug("Get Cache for <{0}>".Formatted(name));
            return (MemoryCache<T>)MemoryCache[name];
        }
    }
}
