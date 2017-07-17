using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Events.Stats;
using SalesApp.Core.Framework;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Stats;
using SalesApp.Core.Services.Stats.Sales;

namespace SalesApp.Core.ViewModels.Stats.Sales
{
    public class SalesStatsFragmentViewModel : StatsFragmentBaseViewModel
    {
        private readonly ICulture _culture = Resolver.Instance.Get<ICulture>();

        private IDeviceResource _deviceResource;
        private Period _currentPeriod = Period.Day;

        public EventHandler<StatsListFetchedEvent> DataFetched;

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

        public SalesStatsFragmentViewModel()
        {
            this._deviceResource = Resolver.Instance.Get<IDeviceResource>();

            this.Columns = new ObservableCollection<string>
            {
                this._deviceResource.ColumnDay,
                this._deviceResource.ColumnSales,
                this._deviceResource.ColumnProspects
            };
        }

        public void SetCurrentPeriod(int position)
        {
            switch (position)
            {
                case 0:
                    this.CurrentPeriod = Period.Day;
                    break;
                case 1:
                    this.CurrentPeriod = Period.Week;
                    break;
                case 2:
                    this.CurrentPeriod = Period.Month;
                    break;
            }
        }

        private async Task<List<Block>> LoadSummaryData()
        {
            LocalSalesStatsService salesStatsService = new LocalSalesStatsService();
            DateTime lastUpdated = await salesStatsService.GetLastUpdatedTime();
            int salesToday = await salesStatsService.GetSalesTodayAsync();
            int salesThisWeek = await salesStatsService.GetSalesThisWeekAsync();
            int salesThisMonth = await salesStatsService.GetSalesThisMonth();

            List<Block> blocks = new List<Block>
            {
                new Block
                {
                    Level = 0,
                    BottomValue = string.Empty,
                    Caption = string.Empty,
                    TopValue = salesToday.ToString(),
                    LastUpdateTime = lastUpdated
                },
                new Block
                {
                    Level = 1,
                    Caption = string.Empty,
                    BottomValue = string.Empty,
                    TopValue = salesThisWeek.ToString(),
                    LastUpdateTime = lastUpdated
                },
                new Block
                {
                    Level = 2,
                    Caption = string.Empty,
                    BottomValue = string.Empty,
                    TopValue = salesThisMonth.ToString(),
                    LastUpdateTime = lastUpdated
                }
            };

            for (int i = 0; i < blocks.Count; i++)
            {
                string caption = this.GetBlockCaption(i);
                blocks[i].Caption = caption;
            }

            this.Summary = new ObservableCollection<Block>(blocks);
            return blocks;
        }

        private async Task<List<Row>> LoadListData(bool forceRemote)
        {
            LocalSalesStatsService salesStatsService = new LocalSalesStatsService();

            switch (this.CurrentPeriod)
            {
                case Period.Day:
                    this.Columns[0] = this._deviceResource.ColumnDay;
                    break;
                case Period.Week:
                    this.Columns[0] = this._deviceResource.ColumnWeek;
                    break;
                case Period.Month:
                    this.Columns[0] = this._deviceResource.ColumnMonth;
                    break;
            }

            var stats = await salesStatsService.GetAggregatedStats(this.CurrentPeriod);

            if (stats == null || stats.Count == 0 || forceRemote)
            {
                stats = await new RemoteSalesStatsService().UpdateStats(this.CurrentPeriod);
            }

            List<Row> rows = new List<Row>();

            if (stats != null && stats.Count > 0)
            {
                foreach (var aggStat in stats)
                {
                    var row = new Row
                    {
                        IsSelected = false,
                        Items = new List<string>
                        {
                            this.GetPeriodDateString(aggStat.From, aggStat.To),
                            aggStat.Sales.ToString(),
                            aggStat.Prospects.ToString()
                        }
                    };

                    rows.Add(row);
                }
            }

            if (this.DataFetched != null)
            {
                this.DataFetched.Invoke(this, new StatsListFetchedEvent(rows));
            }

            return rows;
        }

        public async Task LoadSalesData(bool forceRemote = false)
        {
            await this.LoadListData(forceRemote);
            await this.LoadSummaryData();
        }

        private string GetPeriodDateString(DateTime from, DateTime to)
        {
            string weekFormat = this._culture.GetShortDateFormat();

            switch (this.CurrentPeriod)
            {
                case Period.Month:
                    return from.ToString("MMMM");
                case Period.Week:
                    return string.Format("{0} - {1}", from.ToString(weekFormat), to.ToString(weekFormat));
                case Period.Day:
                    return string.Format("{0}", from.Date.ToString(weekFormat));
            }

            return string.Format("{0} - {1}", from, to);
        }

        private string GetBlockCaption(int block)
        {
            switch (block)
            {
                case 0:
                    return this._deviceResource.StatsToday;
                case 1:
                    return this._deviceResource.StatsThisWeek;
                case 2:
                    return this._deviceResource.StatsThisMonth;
                default:
                    return "Unknown Caption";
            }
        }
    }
}