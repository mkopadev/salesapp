using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Reporting;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Events.Stats.Reporting;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Stats;
using SalesApp.Core.Services.Stats.Aggregated;

namespace SalesApp.Core.ViewModels.Stats.Reporting
{
    public class ReportingLevelStatsFragmentViewModel : StatsFragmentBaseViewModel
    {
        private ReportingLevelEntity _currentEntity;
        private ReportingLevelStatsService _reportingLevelStatsService;
        private SelectionCache _selectionCache;

        private string _title = "Title";
        private string _selectedPeriod;
        private bool _levelUpButtonEnabled;
        private bool _hasPrevious;
        private bool _hasNext;

        public event EventHandler<ReportingStatsDataFetchedEvent> DataFetched;

        public ReportingLevelStatsFragmentViewModel(ReportingLevelStatsService service)
        {
            this._reportingLevelStatsService = service;

            IDeviceResource deviceResource = Resolver.Instance.Get<IDeviceResource>();

            this.Columns = new ObservableCollection<string>
            {
                deviceResource.StatsRank,
                deviceResource.StatsNameHeader,
                deviceResource.ColumnSales,
                deviceResource.ColumnProspects
            };
        }

        public bool HasNext
        {
            get
            {
                return this._hasNext;
            }

            set
            {
                this.SetProperty(ref this._hasNext, value, () => this.HasNext);
            }
        }

        public string PreviousPeriod
        {
            get
            {
                return this._reportingLevelStatsService.PreviousPeriod(
                    this._currentEntity.ReportStats,
                    this._selectionCache.SelectedPeriod);
            }
        }

        public string NextPeriod
        {
            get
            {
                return this._reportingLevelStatsService.NextPeriod(
                    this._currentEntity.ReportStats,
                    this._selectionCache.SelectedPeriod);
            }
        }

        public bool HasPrevious
        {
            get
            {
                return this._hasPrevious;
            }

            set
            {
                this.SetProperty(ref this._hasPrevious, value, () => this.HasPrevious);
            }
        }

        public bool LevelUpButtonEnabled
        {
            get
            {
                return this._levelUpButtonEnabled;
            }

            set
            {
                this.SetProperty(ref this._levelUpButtonEnabled, value, () => this.LevelUpButtonEnabled);
            }
        }

        public string SelectedPeriod
        {
            get
            {
                return this._selectedPeriod;
            }

            set
            {
                this.SetProperty(ref this._selectedPeriod, value, () => this.SelectedPeriod);
            }
        }

        public string Title
        {
            get
            {
                return this._title;
            }

            set
            {
                this.SetProperty(ref this._title, value, () => this.Title);
            }
        }

        public string GetSelectedPeriod(ReportStat[] list)
        {
            return this._selectionCache.SelectedPeriod ?? this._reportingLevelStatsService.FirstPeriod(list);
        }

        public async Task<SelectionCache> GetSelectionCache()
        {
            SelectionCache cache = await this._reportingLevelStatsService.RetrieveSelection();

            if (cache == null)
            {
                return new SelectionCache();
            }

            return cache;
        }

        public async Task<ReportingLevelEntity> GetReportingStats(bool ignoreCache = false)
        {
            ReportingLevelEntity entity;
            this._selectionCache = await this.GetSelectionCache();

            if (Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                entity = await this._reportingLevelStatsService.RetrieveStats(this._selectionCache, ignoreCache);
            }
            else
            {
                entity = await this._reportingLevelStatsService.RetrieveStats(this._selectionCache);
            }

            if (entity == null || !entity.HasValidData)
            {
                if (entity == null)
                {
                    entity = new ReportingLevelEntity();
                }

                this.InvokeDataFetched(entity);
                return entity;
            }

            this.UpdateSummary(entity.Sales);
            this.InvokeDataFetched(entity);

            this._currentEntity = entity;
            this.Title = entity.Name;
            this.LevelUpButtonEnabled = this._selectionCache.SelectedLevel != (int)SalesAreaHierarchy.Country;

            if (this._selectionCache.SelectedPeriod == null)
            {
                this._selectionCache.SelectedPeriod = this._reportingLevelStatsService.FirstPeriod(entity.ReportStats);
            }

            this.SelectedPeriod = this._selectionCache == null ? "-" : this._selectionCache.SelectedPeriod;
            var hasPrevious = this._reportingLevelStatsService.PreviousPeriod(entity.ReportStats, this.SelectedPeriod) != null;
            this.HasPrevious = hasPrevious;

            var selectedPeriod = this._selectionCache != null ? this._selectionCache.SelectedPeriod : string.Empty;
            var hasNext = this._reportingLevelStatsService.NextPeriod(entity.ReportStats, selectedPeriod) != null;
            this.HasNext = hasNext;

            return entity;
        }

        private void UpdateSummary(ReportingLevelEntity.Sale[] sales)
        {
            if (sales == null)
            {
                return;
            }

            List<Block> summary = new List<Block>();
            int x = 0;

            foreach (var sale in sales)
            {
                summary.Add(new Block
                {
                    TopValue = sale.Value,
                    Level = x++,
                    Caption = sale.Name
                });
            }

            this.Summary = new ObservableCollection<Block>(summary);
        }

        private void InvokeDataFetched(ReportingLevelEntity entity)
        {
            if (this.DataFetched == null)
            {
                return;
            }

            this.DataFetched.Invoke(this, new ReportingStatsDataFetchedEvent(entity));
        }

        public void UpdateSelectedLevel(int selectedLevel)
        {
            if (this._selectionCache == null)
            {
                return;
            }

            this._selectionCache.SelectedLevel = selectedLevel;
            this._reportingLevelStatsService.StoreSelection(this._selectionCache);
        }

        public void UpdateSelectedItemId(string itemId)
        {
            if (this._selectionCache == null)
            {
                return;
            }

            this._selectionCache.SelectedItemId = itemId;
            this._reportingLevelStatsService.StoreSelection(this._selectionCache);
        }

        public void UpdatePeriodType(int position)
        {
            if (this._selectionCache == null)
            {
                return;
            }

            switch (position)
            {
                case 0:
                    this._selectionCache.SelectedPeriodType = Period.Day;
                    break;
                case 1:
                    this._selectionCache.SelectedPeriodType = Period.Week;
                    break;
                case 2:
                    this._selectionCache.SelectedPeriodType = Period.Month;
                    break;
            }

            this._reportingLevelStatsService.StoreSelection(this._selectionCache);
        }

        public void UpdatedSelectedPeriod(string value)
        {
            if (this._selectionCache == null)
            {
                return;
            }

            this._selectionCache.SelectedPeriod = value;
            this._reportingLevelStatsService.StoreSelection(this._selectionCache);
        }

        public void GoPrevious()
        {
            if (this.PreviousPeriod == null)
            {
                return;
            }

            this._selectionCache.SelectedPeriod = PreviousPeriod;
            this._reportingLevelStatsService.StoreSelection(this._selectionCache);

            this.GetReportingStats();
        }

        public void GoNext()
        {
            if (this.NextPeriod == null)
            {
                return;
            }

            this._selectionCache.SelectedPeriod = this.NextPeriod;
            this._reportingLevelStatsService.StoreSelection(this._selectionCache);

            this.GetReportingStats();
        }

        public ReportingLevelEntity.ParentItem GetParentItem()
        {
            return this._currentEntity.Parent;
        }
    }
}