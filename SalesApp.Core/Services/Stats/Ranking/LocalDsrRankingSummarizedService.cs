using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.Stats;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Stats.Ranking
{
    public class LocalDsrRankingSummarizedService
    {
        private DsrSummarizedRankingController _summarizedRankingController;

        public LocalDsrRankingSummarizedService()
        {
            this.Logger = Resolver.Instance.Get<ILog>();
            this.Logger.Initialize(this.GetType().FullName);
        }

        private DsrSummarizedRankingController SummarizedRankingController
        {
            get
            {
                this._summarizedRankingController = this._summarizedRankingController ?? new DsrSummarizedRankingController();
                return this._summarizedRankingController;
            }
        }

        private ILog Logger { get; set; }

        public async Task<bool> SetAsync(List<RankingSummarized> summarizedRankings)
        {
            if (summarizedRankings == null || summarizedRankings.Count == 0)
            {
                return true;
            }

            await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async tran =>
                    {
                        DataAccess.Instance.StartTransaction(tran);
                        await DataAccess.Instance.DeleteAllAsync<RankingSummarized>();
                        foreach (var summaryRanking in summarizedRankings)
                        {
                            await this.SummarizedRankingController.SaveAsync(tran, summaryRanking);
                        }
                    });
            DataAccess.Instance.CommitTransaction();
            return true;
        }

        public async Task<List<RankingSummarized>> GetAsync()
        {
            return await this.SummarizedRankingController.GetAllAsync();
        }

        public async Task<DateTime> GetLastUpdatedTime()
        {
            try
            {
                List<RankingSummarized> latestStat = await SummarizedRankingController.SelectQueryAsync(
                        string.Format(
                                "Select * from RankingSummarized order by strftime('%Y%m%d', datetime(\"{0}\",'localtime')) desc limit 0,1",
                                "DateUpdated"));

                if (latestStat == null || latestStat.Count == 0)
                {
                    return default(DateTime);
                }

                Logger.Debug("Returned ~ results".GetFormated(latestStat.Count));
                DateTime lastDateTime =
                    latestStat.OrderByDescending(date => date.Modified).FirstOrDefault().Modified;
                Logger.Debug("Value was ~ ".GetFormated(lastDateTime));

                return lastDateTime;
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }
        }
    }
}