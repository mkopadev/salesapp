using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL;
using SalesApp.Core.Events.Stats;
using SalesApp.Core.Events.Stats.Units;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Product;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Services.Stats;
using SalesApp.Core.Services.Stats.Units;

namespace SalesApp.Core.ViewModels.Stats.Units
{
    public class UnitsStatsFragmentViewModel : BaseViewModel
    {
        private DateTime _lastUpdatedTime = default(DateTime);
        private string _allocatedUnits = "0";
        private LocalProductService _localProductService;
        private RemoteProductService _remoteProductService;
        private LocalUnitsService _localUnitsService;
        private RemoteUnitsService _remoteUnitsService;

        public EventHandler<StatsListFetchedEvent> UnitsFetched;
        public EventHandler<ProductsFetchedEvent> ProductsFetched;

        public UnitsStatsFragmentViewModel(LocalProductService localProductService, RemoteProductService remoteProductService, LocalUnitsService localUnitsService, RemoteUnitsService remoteUnitsService)
        {
            this._localProductService = localProductService;
            this._remoteProductService = remoteProductService;
            this._localUnitsService = localUnitsService;
            this._remoteUnitsService = remoteUnitsService;
        }

        public string AllocatedUnits
        {
            get
            {
                return this._allocatedUnits;
            }

            set
            {
                this.SetProperty(ref this._allocatedUnits, value, () => this.AllocatedUnits);
            }
        }

        public DateTime LastUpdateTime
        {
            get
            {
                return this._lastUpdatedTime;
            }

            set
            {
                this.SetProperty(ref this._lastUpdatedTime, value, () => this.LastUpdateTime);
            }
        }

        private async Task GetLastUpdateTime()
        {
            DateTime lastUpdate = await this._localProductService.GetLastUpdateTimeAsync();
            this.LastUpdateTime = lastUpdate;
        }

        private async Task GetUnitsAllocatedNow()
        {
            string unitsAllocated = await this._localProductService.GetUnitsAllocatedNowAsync();
            this.AllocatedUnits = unitsAllocated;
        }

        private async Task FetchProducts(bool forceRemote)
        {
            List<Product> products = await this._localProductService.GetAsync("SELECT * FROM Product WHERE SerialNumber NOTNULL ORDER BY DateAcquired DESC");

            if (products == null || products.Count == 0 || forceRemote)
            {
                string dsrPhone = Settings.Instance.DsrPhone;
                string param = string.Format("?dsr={0}", dsrPhone);

                products = await this._remoteProductService.GetProducts(param);
            }

            if (this.ProductsFetched == null)
            {
                return;
            }

            this.ProductsFetched(this, new ProductsFetchedEvent(products));
        }

        private async Task<List<Row>> GetSevenDayHistory(bool forceRemote)
        {
            var units = await this._localUnitsService.GetAsync("SELECT * FROM DsrUnitsInfo Order by Date DESC");

            if (units == null || units.Count == 0 || forceRemote)
            {
                units = await this._remoteUnitsService.UpdateUnitsHistory();
            }

            List<Row> rows = new List<Row>();

            foreach (var unit in units)
            {
                var row = new Row
                {
                    IsSelected = false,
                    Items = new List<string>
                    {
                        this.GetDayFromDate(unit.Date),
                        unit.Date.ToShortDateTime(),
                        unit.NewAcquired.ToString(),
                        unit.StartedWith.ToString(),
                        unit.Removed.ToString()
                    }
                };

                rows.Add(row);
            }

            if (this.UnitsFetched != null)
            {
                this.UnitsFetched(this, new StatsListFetchedEvent(rows));
            }

            return rows;
        }

        private string GetDayFromDate(DateTime dateTime)
        {
            return dateTime.DayOfWeek.ToString();
        }

        public async Task RefreshData(bool forceRemote = false)
        {
            await this.FetchProducts(forceRemote);
            await this.GetLastUpdateTime();
            await this.GetUnitsAllocatedNow();

            await this.GetSevenDayHistory(forceRemote);
        }
    }
}
