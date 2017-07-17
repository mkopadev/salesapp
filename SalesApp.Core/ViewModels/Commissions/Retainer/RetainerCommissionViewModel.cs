using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Commissions.Retainer;

namespace SalesApp.Core.ViewModels.Commissions.Retainer
{
    public class RetainerCommissionViewModel : BaseDetailsViewModel
    {
        private ObservableCollection<RetainerCommissionItem> _commissions;

        // private double sales;
        private string _total;

        public RetainerCommissionViewModel(DateTime currentMonth) : base(currentMonth)
        {
            this.LoadRetainerCommission();
        }

        public ObservableCollection<RetainerCommissionItem> Commmissions
        {
            get
            {
                return this._commissions;
            }

            set
            {
                this.SetProperty(ref this._commissions, value, () => this.Commmissions);
            }
        }

        /*public double Sales
        {
            get
            {
                return this.sales;
            }

            set
            {
                this.SetProperty(ref this.sales, value, () => this.Sales);
            }
        }*/

        public string Total
        {
            get
            {
                return this._total;
            }

            set
            {
                this.SetProperty(ref this._total, value, () => this.Total);
            }
        }

        /*public string TotalSalesLabel
        {
            get
            {
                return "Total Sales (" + this.CurrentMonth.ToShortMonthName().ToLower() + ")";
            }
        }*/

        private async Task LoadRetainerCommission()
        {
            this.Loading = true;

            string urlParams = this.BaseUrlParams + string.Format("&statsType={0}", "Retainer");
            string cacheKey = this.CurrentMonth.ToString("MMMM") + "_Retainer";

            RetainerCommissionResponse data = await this.service.GetCommissionDetails<RetainerCommissionResponse>(urlParams, cacheKey);
            if (data == null || data.Retainer == null || data.Retainer.Count == 0)
            {
                if (data != null)
                {
                    this.Info = data.Info;
                }

                this.HasSales = false;
                this.SalesNotLoaded = true;
            }
            else
            {
                this.HasSales = true;
                this.SalesNotLoaded = false;

                this.Commmissions = new ObservableCollection<RetainerCommissionItem>(data.Retainer);
                this.Info = data.Info;

                // this.Sales = data.Sales;
                this.Total = data.Total;
            }

            this.Loading = false;
        }
    }
}