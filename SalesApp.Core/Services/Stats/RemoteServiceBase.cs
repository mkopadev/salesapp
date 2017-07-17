using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Stats
{
    public abstract class RemoteServiceBase<TApiClass, TModel> where TApiClass : ApiBase where TModel : BusinessEntityBase, new()
    {
        private class DataService : SQLiteDataService<TModel>
        {
        }

        protected RemoteServiceBase()
        {
            Logger = Resolver.Instance.Get<ILog>();
            Logger.Initialize(this.GetType().FullName);
        }

        public TApiClass Api { get; set; }

        protected ILog Logger { get; set; }

        protected bool HandleException(Exception exception, string message)
        {
            Type[] handledExceptions = new Type[]
            {
                typeof (NotConnectedToInternetException)
                , typeof (JsonReaderException)
                , typeof (TaskCanceledException)
            };
            bool handled = handledExceptions.Contains(exception.GetType()) || exception.InnerException == null
                ? false
                : handledExceptions.Contains(exception.InnerException.GetType());

            if (handled)
            {
                if (!message.IsBlank())
                {
                    Logger.Error(message);
                }

                Logger.Error(exception);

            }
            return handled;

        }



        public async Task<DateTime> GetLastUpdateDateTimeAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return default(DateTime).AddDays(1);
            }

            DataService dataService = new DataService();
            List<TModel> items = await dataService.GetAllAsync();
            if (items == null || items.Count == 0)
            {
                return default(DateTime);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return default(DateTime).AddDays(1);
            }

            return items.OrderByDescending(item => item.Modified).First().Modified;
        }
    }
}