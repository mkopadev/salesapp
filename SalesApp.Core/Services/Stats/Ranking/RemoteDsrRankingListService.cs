using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Controllers.Stats;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Services.Stats.Ranking
{
    public class RemoteDsrRankingListService : RemoteServiceBase<DsrRankingListApi, DsrRankInfo, DsrRankingList>
    {
        private DsrRankingListController _rankingListController;

        private LocalRankingService<DsrRankInfo> _localRankingService;

        private DsrRankingListApi RankingListApi
        {
            get
            {
                Api = Api ?? new DsrRankingListApi();
                return Api;
            }
        }

        private DsrRankingListController RankingListController
        {
            get
            {
                _rankingListController = _rankingListController ?? new DsrRankingListController();
                return _rankingListController;
            }
        }

        private LocalRankingService<DsrRankInfo> RankingService
        {
            get
            {
                _localRankingService = _localRankingService ?? new LocalRankingService<DsrRankInfo>();
                return _localRankingService;
            }
        }

        public async Task<DateTime> GetLastUpdateDateTimeAsync(Period period, SalesAreaHierarchy salesArea, CancellationToken cancellationToken)
        {
            if (period == default(Period) && salesArea == default(SalesAreaHierarchy))
            {
                this.Logger.Debug("Hmm... now ain't it strange both period and sales area aren't set?");
                return default(DateTime);
            }

            string query = string.Format("Select * from DsrRankInfo where {0} {1} {2} order by strftime('%Y%m%d', datetime(\"{3}\",'localtime')) desc limit 0,1", period != default(Period) ? "period = '" + (int)period + "'" : string.Empty, (period != default(Period) || salesArea != default(SalesAreaHierarchy)) ? "and" : string.Empty, "Region = '" + (int)salesArea + "'", "DateUpdated");

            if (cancellationToken.IsCancellationRequested)
            {
                this.Logger.Debug("Cancelation was requested");
                return default(DateTime).AddDays(1);
            }

            var latestStat = await this.RankingListController.SelectQueryAsync(query);

            this.Logger.Debug("Finished call to db to check latest record");
            if (latestStat == null || latestStat.Count == 0)
            {
                return default(DateTime);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return default(DateTime).AddDays(1);
            }

            return latestStat.FirstOrDefault().Modified;
        }

        public async Task<List<Row>> GetListAsync(Period period, SalesAreaHierarchy region)
        {
            string jsonString = string.Empty;
            try
            {
                string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

                string urlParams = string.Format("{0}?periodType={1}&region={2}", userId, (int)period, (int)region);

                ServerResponse<DsrRankingList> serverResponse = await this.RankingListApi.MakeGetCallAsync<DsrRankingList>(urlParams, filterFlags: ErrorFilterFlags.AllowEmptyResponses | ErrorFilterFlags.IgnoreNoInternetError);

                if (serverResponse == null)
                {
                    return new List<Row>();
                }

                if (serverResponse.IsSuccessStatus)
                {
                    jsonString = serverResponse.RawResponse;
                    DsrRankingList rankingList = serverResponse.GetObject();
                    foreach (var rankInfo in rankingList.Dsrs)
                    {
                        rankInfo.TimeStamp = rankingList.TimeStamp;
                        rankInfo.Period = period;
                        rankInfo.Region = region;
                    }

                    await this.RankingService.SetAsync(rankingList.Dsrs, period, region);
                    return await new RankingsRowService().GetRankingRows(period, region);
                }

                return new List<Row>();
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.HandleException(jsonReaderException, "Attempt to parse invalid JSON may have been made." + " JSON: " + jsonString);
            }
            catch (NotConnectedToInternetException notConnectedToInternetException)
            {
                this.HandleException(notConnectedToInternetException, "Unable to connect internet. Could connection have dropped?");
            }
            catch (TaskCanceledException taskCanceled)
            {
                this.HandleException(taskCanceled, "Timeout may have occured or task may have been explicitly canceled by user.");
            }
            catch (Exception exception)
            {
                this.HandleException(exception, string.Empty);
            }

            return new List<Row>();
        }
    }
}