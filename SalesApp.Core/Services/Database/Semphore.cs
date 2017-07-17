using System;
using System.Threading;

namespace SalesApp.Core.Services.Database
{

    public class Semphore
    {
        private static readonly Lazy<Semphore> INSTANCE = new Lazy<Semphore>(() => new Semphore());

        private SemaphoreSlim _semphore = new SemaphoreSlim(1);


        private Semphore()
        {
            
        }

        public SemaphoreSlim Get()
        {
            return _semphore;
        }
        public static Semphore Instance
        {
            get { return INSTANCE.Value; }
        }
    }
}
