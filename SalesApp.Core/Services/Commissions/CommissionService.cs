using System;
using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Commissions;
using SalesApp.Core.BL.Cache;
using SalesApp.Core.BL.Models.Commissions.Summary;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Commissions
{
    public class CommissionService
    {
        private CommissionApi _commissionApi;
        private ILog _logger = LogManager.Get(typeof(CommissionService));

        public CommissionService(CommissionApi api)
        {
            this._commissionApi = api;
        }

        public async Task<CommissionSummaryResponse> GetCommissionSummary(string param, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("memory caching key cannot be null");
            }

            // check the object in cache
            var response = await MemoryCache.Instance.Get<CommissionSummaryResponse>(key);
            if (response != null)
            {
                return response;
            }

            if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                this._logger.Debug("No connectivity");
                throw new NotConnectedToInternetException();
            }

            ServerResponse<CommissionSummaryResponse> serverResponse = await this._commissionApi.MakeGetCallAsync<CommissionSummaryResponse>(param);

            if (serverResponse.RequestException != null)
            {
                // check for 401, and throw unauthorized exception
                UnauthorizedHttpException uex = serverResponse.RequestException as UnauthorizedHttpException;
                if (uex != null)
                {
                    throw uex;
                }
            }

            response = serverResponse.GetObject();

            // save in cache
            if (response != null)
            {
                MemoryCache.Instance.Store(key, response);
            }

            return response;
        }

        public async Task<T> GetCommissionDetails<T>(string param, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("memory caching key cannotbe null");
            }

            // check the object in cache
            var response = await MemoryCache.Instance.Get<T>(key);
            if (response != null)
            {
                return response;
            }

            if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                this._logger.Debug("No connectivity");
                return default(T);
            }

            ServerResponse<T> serverResponse = await this._commissionApi.MakeGetCallAsync<T>(param);
            response = serverResponse.GetObject();

            // save object in cache
            if (response != null)
            {
                MemoryCache.Instance.Store(key, response);
            }

            return response;
        }
    }
}