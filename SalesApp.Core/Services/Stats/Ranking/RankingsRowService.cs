using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Enums.Stats;

namespace SalesApp.Core.Services.Stats.Ranking
{
    public class RankingsRowService
    {
        public async Task<List<Row>> GetRankingRows(Period period, SalesAreaHierarchy region)
        {
            LocalRankingService<DsrRankInfo> localRanking = new LocalRankingService<DsrRankInfo>();
            List<DsrRankInfo> ranks = await localRanking.GetListByPeriodAndArea(period, region);
            List<Row> rows = new List<Row>();

            foreach (var rank in ranks)
            {
                var row = new Row
                {
                    IsSelected = rank.IsMe,
                    Items = new List<string>()
                    {
                        rank.Rank.ToString(),
                        rank.Name,
                        rank.Sales.ToString()
                    }
                };

                rows.Add(row);
            }

            return rows;
        }
    }
}
