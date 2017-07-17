using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Ranking;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Events.Stats;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Stats;
using SalesApp.Core.Services.Stats.Ranking;

namespace SalesApp.Core.ViewModels.Stats.Rankings
{
    public class RankingStatsListFragmentViewModel : StatsFragmentBaseViewModel
    {
        public event EventHandler<StatsListFetchedEvent> DataFetched;

        private SalesAreaHierarchy _currentRegion = SalesAreaHierarchy.ServiceCentre;
        private Period _currentPeriod = Period.Month;

        public SalesAreaHierarchy CurrentRegion
        {
            get
            {
                return this._currentRegion;
            }

            set
            {
                this._currentRegion = value;
            }
        }

        public Period CurrentPeriod
        {
            get
            {
                return this._currentPeriod;
            }

            set
            {
                this._currentPeriod = value;
            }
        }

        public RankingStatsListFragmentViewModel()
        {
            IDeviceResource deviceResource = Resolver.Instance.Get<IDeviceResource>();

            this.Columns = new ObservableCollection<string>
            {
                deviceResource.StatsRank,
                deviceResource.StatsDsr,
                deviceResource.StatsSales
            };
        }

        public void UpdateCurrentRegion(int position)
        {
            switch (position)
            {
                case 0:
                    this.CurrentRegion = SalesAreaHierarchy.ServiceCentre;
                    break;
                case 1:
                    this.CurrentRegion = SalesAreaHierarchy.Region;
                    break;
                case 2:
                    this.CurrentRegion = SalesAreaHierarchy.All;
                    break;
            }
        }

        private async Task<List<RankingSummarized>> FetchRemoteRankingSummary()
        {
            List<RankingSummarized> rankingSummary = await new RemoteDsrRankingSummarizedService().GetSummarizedRankAsync(this.CurrentPeriod, this.CurrentRegion);

            return rankingSummary;
        }

        private async Task<List<Block>> UpdateSummaryBlocks(bool forceRemote = false)
        {
            try
            {
                LocalDsrRankingSummarizedService localDsrRanking = new LocalDsrRankingSummarizedService();
                List<RankingSummarized> summarizedRanks = await localDsrRanking.GetAsync();
                List<Block> blocks = new List<Block>();

                if (summarizedRanks == null || summarizedRanks.Count == 0 || forceRemote)
                {
                    summarizedRanks = await this.FetchRemoteRankingSummary();
                }

                DateTime lastUpdated = await localDsrRanking.GetLastUpdatedTime();

                blocks.AddRange(summarizedRanks.Select(t => new Block
                {
                    Level = t.Level,
                    Caption = t.Area,
                    TopValue = t.DsrRank.ToString(),
                    BottomValue = t.TotalDsrs.ToString(),
                    LastUpdateTime = lastUpdated
                }));

                this.Summary = new ObservableCollection<Block>(blocks);
                return blocks;
            }
            catch (Exception)
            {
                return new List<Block>();
            }
        }

        private async Task<List<Row>> FetchRemoteRankingList()
        {
            var rows = await new RemoteDsrRankingListService().GetListAsync(this.CurrentPeriod, this.CurrentRegion);
            return rows;
        }

        public async Task LoadRankingsData(bool forceRemote = true)
        {
            List<Row> rows = await new RankingsRowService().GetRankingRows(this.CurrentPeriod, this.CurrentRegion);

            if (rows == null || rows.Count == 0 || forceRemote)
            {
                rows = await this.FetchRemoteRankingList();
            }

            await this.UpdateSummaryBlocks(forceRemote);

            if (this.DataFetched == null)
            {
                return;
            }

            this.DataFetched(this, new StatsListFetchedEvent(rows));
        }
    }
}
