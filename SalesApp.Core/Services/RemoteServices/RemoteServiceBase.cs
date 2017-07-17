using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.RemoteServices
{
    public class RemoteServiceBase<TApiClass,TModel,TServerResponse> 
        where TApiClass : ApiBase
        where TModel : BusinessEntityBase , new()
    {
        public event EventHandler<ExceptionHandledEventArgs> ExceptionHandled;
 
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
            Logger.Debug("Exception message '~'".GetFormated(message));
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
            if (handled && ExceptionHandled != null)
            {
                ExceptionHandled(this,new ExceptionHandledEventArgs(exception));
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

        public async virtual Task<ServerResponse<TServerResponse>> GetRawServerReponseAsync(string queryString, ErrorFilterFlags flags)
        {
            Logger.Debug("Making get call for remote service ~".GetFormated(GetType().Name));
            Logger.Debug("Get query string is ~".GetFormated(queryString));
            string jsonString = "";
            try
            {
                if (Api == null)
                {
                    Api = Activator.CreateInstance<TApiClass>();
                }

                ServerResponse<TServerResponse> serverResponse =
                    await this.Api.MakeGetCallAsync<TServerResponse>(queryString, null, flags);

                if (serverResponse == null)
                {
                    Logger.Verbose("No results.");
                }
                else if (serverResponse.IsSuccessStatus)
                {
                    jsonString = serverResponse.RawResponse;
                    return serverResponse;
                }

            }
            catch (JsonReaderException jsonReaderException)
            {
                HandleException
                    (
                        jsonReaderException
                        , "Attempt to parse invalid JSON may have been made."
                          + " JSON: " + jsonString
                    );


            }
            catch (NotConnectedToInternetException notConnectedToInternetException)
            {
                HandleException
                    (
                        notConnectedToInternetException
                        , "Unable to connect internet. Could connection have dropped?"
                    );

            }
            catch (TaskCanceledException taskCanceled)
            {
                HandleException(taskCanceled, "Timeout may have occured or task may have been explicitly canceled by user.");


            }
            catch (Exception exception)
            {
                HandleException(exception, "Exception: " + exception.Message);
            }
            return new ServerResponse<TServerResponse>
            {
                IsSuccessStatus = false
                ,
                RawResponse = JsonConvert.SerializeObject(Activator.CreateInstance<TServerResponse>())
                ,
                StatusCode = HttpStatusCode.Ambiguous
                ,
                RequestException = null
            };
        }

        public async virtual Task<TServerResponse> GetAsync(string queryString, ErrorFilterFlags flags = ErrorFilterFlags.EnableErrorHandling)
        {
            var result = await GetRawServerReponseAsync(queryString, flags);
            return result.GetObject();
        }
    }
}