using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Services.Stats.Ranking
{
    public class RemoteDsrRankingSummarizedService : RemoteServiceBase<DsrRankingSummarizedApi, RankingSummarized, List<RankingSummarized>>
    {
        private LocalDsrRankingSummarizedService _localDsrRanking;

        private DsrRankingSummarizedApi SummarizedRanksApi
        {
            get
            {
                Api = Api ?? new DsrRankingSummarizedApi();
                return Api;
            }
        }

        private LocalDsrRankingSummarizedService LocalDsrRanking
        {
            get
            {
                _localDsrRanking = _localDsrRanking ?? new LocalDsrRankingSummarizedService();
                return _localDsrRanking;
            }
        }

        public async Task<List<RankingSummarized>> GetSummarizedRankAsync(Period period, SalesAreaHierarchy region)
        {
            try
            {
                DateTime lastUpdate = await this.GetLastUpdateDateTimeAsync(new CancellationTokenSource().Token);

                int validityMinutes = Settings.Settings.Instance.RankingSummarizedValidityMinutes;
                if (lastUpdate > default(DateTime))
                {
                    if (lastUpdate.AddMinutes(validityMinutes) > DateTime.Now)
                    {
                        return new List<RankingSummarized>();
                    }
                }

                string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

                string urlParams = string.Format("{0}?periodType={1}&region={2}", userId, (int)period, (int)region);

                ServerResponse<List<RankingSummarized>> serverResponse = await this.SummarizedRanksApi.MakeGetCallAsync<List<RankingSummarized>>(urlParams, filterFlags: ErrorFilterFlags.AllowEmptyResponses | ErrorFilterFlags.IgnoreNoInternetError);
                if (serverResponse == null)
                {
                    return new List<RankingSummarized>();
                }

                if (serverResponse.IsSuccessStatus)
                {
                    var result = serverResponse.GetObject();
                    await this.LocalDsrRanking.SetAsync(result);
                    return result;
                }

                return new List<RankingSummarized>();
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.Logger.Error(jsonReaderException);
            }
            catch (NotConnectedToInternetException nctiex)
            {
                this.Logger.Error(nctiex);
            }
            catch (TaskCanceledException taskCanceled)
            {
                this.Logger.Error(taskCanceled);
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
            }

            return new List<RankingSummarized>();
        }
    }
}