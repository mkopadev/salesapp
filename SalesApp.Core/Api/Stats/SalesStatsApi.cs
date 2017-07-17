using System;
using System.Threading.Tasks;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Stats.Sales;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Api.Stats
{
    public class SalesStatsApi : ApiBase
    {
        public SalesStatsApi() : this("SalesStats/")
        {
        }

        private SalesStatsApi(string apiRelativePath) : base(apiRelativePath)
        {
        }

        public async Task<ServerResponse<StatHeader>> FetchStats(DateTime from, DateTime to)
        {
            return await this.FetchStats(Period.Day, from, to);
        }

        protected async Task<ServerResponse<StatHeader>> FetchStats(Period period, DateTime from, DateTime to)
        {
            string apiFormat = "yyyy-MM-ddThh:mm";
            ISalesAppSession session = Resolver.Instance.Get<ISalesAppSession>();
            Guid userId = session.UserId;
            ServerResponse<StatHeader> serverResponse;
            try
            {
                string url = string.Format("{0}?periodType={1}&from={2}&to={3}", userId, (int)period,  @from.ToString(apiFormat), to.ToString(apiFormat));
                serverResponse = await MakeGetCallAsync<StatHeader>(url, filterFlags: ErrorFilterFlags.IgnoreNoInternetError);
            }
            catch (TimeoutException timeoutException)
            {
                this.Logger.Error(timeoutException);
                return null;
            }
            catch (NotConnectedToInternetException exception)
            {
                this.Logger.Error(exception);
                return null;
            }

            if (serverResponse == null || serverResponse.IsSuccessStatus == false)
            {
                return null;
            }

            return serverResponse;
        }
    }
}